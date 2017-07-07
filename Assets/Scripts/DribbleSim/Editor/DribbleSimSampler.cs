using UnityEngine;
using UnityEditor;

public static class DribbleSimSampler
{
    public const string NODE_PATH_LEFT_BALL = "Root/Bip001/Hips/Spine/Spine1/Chest/LeftShoulder/LeftArm/LeftForeArm/LeftHand/Leftball";
    public const string NODE_PATH_RIGHT_BALL = "Root/Bip001/Hips/Spine/Spine1/Chest/RightShoulder/RightArm/RightForeArm/RightHand/Rightball";

    public static void Sample(GameObject[] bodies, AnimationClip[] clips)
    {
        string filename = "Assets/Resources/Data/Dribble.asset";
        DribbleData data = AssetDatabase.LoadAssetAtPath<DribbleData>(filename);
        if (data == null)
        {
            data = ScriptableObject.CreateInstance<DribbleData>();
            AssetDatabase.CreateAsset(data, filename);
        }
        data.Clips.Clear();

        foreach (var clip in clips)
        {
            //Debug.LogFormat("导出运球数据，Clip: {0}", clip.name);
            var clipInfo = Sample(bodies, clip);
            data.Clips.Add(clipInfo);
        }

        EditorUtility.SetDirty(data);

        Debug.LogFormat("Dribble simulation data exported.\n{0}", data);
    }

    public static DribbleData.Clip Sample(GameObject[] bodies, AnimationClip clip)
    {
        var clipSettings = AnimationUtility.GetAnimationClipSettings(clip);
        DribbleData.Clip clipInfo = new DribbleData.Clip()
        {
            ClipName = clip.name,
            NameHash = Animator.StringToHash(clip.name),
        };
        DribbleData.Entry curEventInfo = null;
        foreach (var evt in clip.events)
        {
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

                for (int i = 0; i < (int)BodyType.Count; ++i)
                {
                    GameObject body = bodies[i];
                    Debug.AssertFormat(body != null, "Body is null. {0}", (BodyType)i);

                    AnimationMode.BeginSampling();
                    AnimationMode.SampleAnimationClip(body, clip, evt.time);
                    AnimationMode.EndSampling();

                    Transform leftBall = body.transform.Find(NODE_PATH_LEFT_BALL);
                    if (leftBall == null)
                    {
                        Debug.LogErrorFormat("GameObject: {0} has no bone Leftball.", body.name);
                        return null;
                    }
                    Transform rightBall = body.transform.Find(NODE_PATH_RIGHT_BALL);
                    if (rightBall == null)
                    {
                        Debug.LogErrorFormat("GameObject: {0} has no bone Rightball.", body.name);
                        return null;
                    }

                    Vector3 pos = (hand == Hand.Left ? leftBall.position : rightBall.position);
                    if (clipSettings.mirror)
                        pos.x = -pos.x;
                    curEventInfo.OutPosition[i] = pos;
                }
            }
            else if (evt.functionName == "DribbleIn")
            {
                if (curEventInfo == null)
                {
                    Debug.LogErrorFormat("Out与In不匹配, clip:{0}", clip.name);
                    return null;
                }

                Hand hand;
                DribbleSimulator.TryParseInInfo(evt.stringParameter, out hand);
                curEventInfo.InHand = clipSettings.mirror ? DribbleSimulator.MirrorHand(hand) : hand;
                //curEventInfo.InHand = hand;
                curEventInfo.InTime = evt.time;
                curEventInfo.InNormalizedTime = evt.time / clip.length;

                for (int i = 0; i < (int)BodyType.Count; ++i)
                {
                    GameObject body = bodies[i];
                    Debug.AssertFormat(body != null, "Body is null. {0}", (BodyType)i);

                    AnimationMode.BeginSampling();
                    AnimationMode.SampleAnimationClip(body, clip, evt.time);
                    AnimationMode.EndSampling();

                    Transform leftBall = body.transform.Find(NODE_PATH_LEFT_BALL);
                    if (leftBall == null)
                    {
                        Debug.LogErrorFormat("GameObject: {0} has no bone Leftball.", body.name);
                        return null;
                    }
                    Transform rightBall = body.transform.Find(NODE_PATH_RIGHT_BALL);
                    if (rightBall == null)
                    {
                        Debug.LogErrorFormat("GameObject: {0} has no bone Rightball.", body.name);
                        return null;
                    }

                    Vector3 pos = (hand == Hand.Left ? leftBall.position : rightBall.position);
                    if (clipSettings.mirror)
                        pos.x = -pos.x;
                    curEventInfo.InPosition[i] = pos;
                }

                clipInfo.Entries.Add(curEventInfo);
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
        return clipInfo;
    }
}
