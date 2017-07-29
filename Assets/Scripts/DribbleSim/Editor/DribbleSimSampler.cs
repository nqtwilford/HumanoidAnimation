using UnityEngine;
using UnityEditor;

public static class DribbleSimSampler
{
    public static DribbleData Sample(AnimationClip[] clips)
    {
        string filename = "Assets/Resources/Data/Dribble.asset";
        DribbleData data = AssetDatabase.LoadAssetAtPath<DribbleData>(filename);
        if (data == null)
        {
            data = ScriptableObject.CreateInstance<DribbleData>();
            AssetDatabase.CreateAsset(data, filename);
        }
        data.Clear();

        foreach (var clip in clips)
        {
            //Debug.LogFormat("导出运球数据，Clip: {0}", clip.name);
            var clipInfo = Sample(clip);
            data.AddClipData(clipInfo);
        }

        EditorUtility.SetDirty(data);

        return data;
    }

    public static DribbleData.Clip Sample(AnimationClip clip)
    {
        var clipSettings = AnimationUtility.GetAnimationClipSettings(clip);
        DribbleData.Clip clipInfo = new DribbleData.Clip()
        {
            ClipName = clip.name,
            NameHash = Animator.StringToHash(clip.name),
        };
        DribbleData.Entry curEventInfo = null;
        AnimationEvent[] events = AnimationUtility.GetAnimationEvents(clip);
        for (int i = 0; i < events.Length; ++i)
        {
            var evt = events[i];
            if (evt.functionName == "DribbleOut")
            {
                DribbleType type;
                Hand hand;
                if (!DribbleSimulator.TryParseOutInfo(evt.stringParameter, out type, out hand))
                {
                    Debug.LogErrorFormat("错误的DribbleOut参数: {0} Clip: {1}", evt.stringParameter, clipInfo.ClipName);
                    return null;
                }

                curEventInfo = new DribbleData.Entry()
                {
                    OutTime = evt.time,
                    OutNormalizedTime = evt.time / clip.length,
                    Type = type,
                    OutHand = clipSettings.mirror ? DribbleSimulator.MirrorHand(hand) : hand,
                    //OutHand = hand,
                };
                events[i].messageOptions = SendMessageOptions.DontRequireReceiver;
            }
            else if (evt.functionName == "BounceGround")
            {
                if (curEventInfo == null)
                {
                    Debug.LogErrorFormat("Out与In不匹配, clip:{0}", clip.name);
                    return null;
                }
                curEventInfo.BounceTime = evt.time;
                curEventInfo.BounceNormalizedTime = evt.time / clip.length;
                events[i].messageOptions = SendMessageOptions.DontRequireReceiver;
            }
            else if (evt.functionName == "DribbleIn")
            {
                if (curEventInfo == null)
                {
                    Debug.LogErrorFormat("Out与In不匹配, clip:{0}", clip.name);
                    return null;
                }
                Debug.Assert(curEventInfo.BounceTime != 0);

                Hand hand;
                DribbleSimulator.TryParseInInfo(evt.stringParameter, out hand);
                curEventInfo.InHand = clipSettings.mirror ? DribbleSimulator.MirrorHand(hand) : hand;
                //curEventInfo.InHand = hand;
                curEventInfo.InTime = evt.time;
                curEventInfo.InNormalizedTime = evt.time / clip.length;
                events[i].messageOptions = SendMessageOptions.DontRequireReceiver;

                clipInfo.AddEntry(curEventInfo);
                curEventInfo = null;
            }
            //GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            //go.name = inst.name + "_left_" + evt.functionName + "_" + evt.time;
            //go.transform.localScale = Vector3.one / 10;
            //go.transform.position = leftBall.position;
            //go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            //go.name = inst.name + "_right_" + evt.functionName + "_" + evt.time;
            //go.transform.localScale = Vector3.one / 10;
            //go.transform.position = rightBall.position;
            //SceneView.RepaintAll();
        }
        AnimationUtility.SetAnimationEvents(clip, events);
        EditorUtility.SetDirty(clip);
        return clipInfo;
    }
}
