using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public static class TargetMatchingSampler
{
    public static void Sample(GameObject go, AnimationClip[] clips)
    {
        string filename = "Assets/Resources/Data/TargetMatching/" + go.name + ".asset";
        TargetMatchingData data = AssetDatabase.LoadAssetAtPath<TargetMatchingData>(filename);
        if (data == null)
        {
            data = ScriptableObject.CreateInstance<TargetMatchingData>();
            AssetDatabase.CreateAsset(data, filename);
        }
        data.Clips.Clear();

        foreach (var clip in clips)
        {
            Debug.LogFormat("导出TargetMatching数据，{0} {1}", go.name, clip.name);
            TargetMatchingData.Clip clipInfo = Sample(go, clip);
            if (clipInfo != null)
                data.Clips.Add(clipInfo);
        }

        EditorUtility.SetDirty(data);
    }

    public static TargetMatchingData.Clip Sample(GameObject go, AnimationClip clip)
    {
        TargetMatchingData.Clip info = null;
        TargetMatchingData.Entry curEntry = null;
        AnimationEvent[] events = AnimationUtility.GetAnimationEvents(clip);
        for (int i = 0; i < events.Length; ++i)
        {
            var evt = events[i];
            if (evt.functionName == "StartMatching")
            {
                var matchingInfo = ParseMatchingInfo(evt.stringParameter);
                AnimationMode.BeginSampling();
                AnimationMode.SampleAnimationClip(go, clip, evt.time);
                AnimationMode.EndSampling();
                Debug.Assert(curEntry == null);

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
                    StartPosition = go.transform.Find("Root/Bip001/Hips").position,
                    PositionWeight = matchingInfo.Value,
                };
            }
            else if (evt.functionName == "EndMatching")
            {
                AnimationMode.BeginSampling();
                AnimationMode.SampleAnimationClip(go, clip, evt.time);
                AnimationMode.EndSampling();

                Debug.Assert(curEntry != null);

                curEntry.TargetTime = evt.time;
                curEntry.TargetNormalizedTime = evt.time / clip.length;
                curEntry.TargetPosition = go.transform.Find("Root/Bip001/Hips").position;

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
