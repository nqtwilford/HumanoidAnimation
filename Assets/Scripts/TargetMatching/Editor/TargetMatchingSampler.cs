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
            Debug.LogFormat("导出TargetMatching数据，{0}", clip.name);
            TargetMatchingData.Clip clipInfo = Sample(bodies, clip);
            if (clipInfo != null)
                data.Clips.Add(clipInfo);
        }

        EditorUtility.SetDirty(data);
    }

    public static TargetMatchingData.Clip Sample(GameObject[] bodies, AnimationClip clip)
    {
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
                    StartTime = evt.time,
                    StartNormalizedTime = evt.time / clip.length,
                    PositionWeight = matchingInfo.Value,
                };

                for (int j = 0; j < (int)BodyType.Count; ++j)
                {
                    GameObject body = bodies[j];
                    AnimationMode.BeginSampling();
                    AnimationMode.SampleAnimationClip(body, clip, evt.time);
                    AnimationMode.EndSampling();
                    curEntry.StartPosition[j] = body.transform.Find("Root/Bip001/Hips").position;
                }
            }
            else if (evt.functionName == "EndMatching")
            {
                Debug.Assert(curEntry != null);
                curEntry.TargetTime = evt.time;
                curEntry.TargetNormalizedTime = evt.time / clip.length;
                for (int j = 0; j < (int)BodyType.Count; ++j)
                {
                    GameObject body = bodies[j];
                    AnimationMode.BeginSampling();
                    AnimationMode.SampleAnimationClip(body, clip, evt.time);
                    AnimationMode.EndSampling();
                    curEntry.TargetPosition[j] = body.transform.Find("Root/Bip001/Hips").position;
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
