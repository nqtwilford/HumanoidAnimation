using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public static class TargetMatchingSampler
{
    public static void Sample(GameObject[] bodies, AnimationClip[] clips)
    {
        string filename = "Assets/Resources/Data/TargetMatching.asset";
        TargetMatchingData data = AssetDatabase.LoadAssetAtPath<TargetMatchingData>(filename);
        if (data == null)
        {
            data = ScriptableObject.CreateInstance<TargetMatchingData>();
            AssetDatabase.CreateAsset(data, filename);
        }
        data.Clips.Clear();

        foreach (var clip in clips)
        {
            //Debug.LogFormat("导出TargetMatching数据，{0}", clip.name);
            TargetMatchingData.Clip clipInfo = Sample(bodies, clip);
            if (clipInfo != null)
                data.Clips.Add(clipInfo);
        }

        EditorUtility.SetDirty(data);

        Debug.LogFormat("Target matching data exported.\n{0}", data);
    }

    public static TargetMatchingData.Clip Sample(GameObject[] bodies, AnimationClip clip)
    {
        var clipSettings = AnimationUtility.GetAnimationClipSettings(clip);
        TargetMatchingData.Clip info = null;
        TargetMatchingData.Entry curEntry = null;
        AnimationEvent[] events = AnimationUtility.GetAnimationEvents(clip);
        for (int i = 0; i < events.Length; ++i)
        {
            var evt = events[i];
            if (evt.functionName == "StartMatching")
            {
                Debug.Assert(curEntry == null);
                var matchingInfo = ParseMatchingInfo(evt.stringParameter);

                if (info == null)
                {
                    info = new TargetMatchingData.Clip()
                    {
                        NameHash = Animator.StringToHash(clip.name),
                        ClipName = clip.name,
                    };
                }
                curEntry = new TargetMatchingData.Entry()
                {
                    TargetBodyPart = matchingInfo.Key,
                    StartNormalizedTime = evt.time / clip.length,
                    PositionWeight = matchingInfo.Value,
                };

                for (int j = 0; j < (int)BodyType.Count; ++j)
                {
                    GameObject body = bodies[j];
                    AnimationMode.BeginSampling();
                    AnimationMode.SampleAnimationClip(body, clip, evt.time);
                    AnimationMode.EndSampling();
                    Vector3 pos = body.transform.Find("Root/Bip001/Hips").position;
                    if (clipSettings.mirror)
                        pos.x = -pos.x;
                    curEntry.StartPosition[j] = pos;
                }
            }
            else if (evt.functionName == "EndMatching")
            {
                Debug.Assert(curEntry != null);
                curEntry.TargetNormalizedTime = evt.time / clip.length;
                for (int j = 0; j < (int)BodyType.Count; ++j)
                {
                    GameObject body = bodies[j];
                    AnimationMode.BeginSampling();
                    AnimationMode.SampleAnimationClip(body, clip, evt.time);
                    AnimationMode.EndSampling();
                    Vector3 pos = body.transform.Find("Root/Bip001/Hips").position;
                    if (clipSettings.mirror)
                        pos.x = -pos.x;
                    curEntry.TargetPosition[j] = pos;
                }

                info.Entries.Add(curEntry);
                events[i].messageOptions = SendMessageOptions.DontRequireReceiver;
                AnimationUtility.SetAnimationEvents(clip, events);
                EditorUtility.SetDirty(clip);
            }
        }
        return info;
    }

    public static KeyValuePair<AvatarTarget, Vector3> ParseMatchingInfo(string args)
    {
        string[] tokens = args.Split(',');
        AvatarTarget target = (AvatarTarget)(Enum.Parse(AvatarTarget.Root.GetType(), tokens[0]));
        Vector3 positionWeight;
        float.TryParse(tokens[1], out positionWeight.x);
        float.TryParse(tokens[2], out positionWeight.y);
        float.TryParse(tokens[3], out positionWeight.z);
        return new KeyValuePair<AvatarTarget, Vector3>(target, positionWeight);
    }
}
