using UnityEngine;
using UnityEditor;

public static class DribbleSimSampler
{
    public const string NODE_PATH_LEFT_BALL = "Root/Bip001/Hips/Spine/Spine1/Chest/LeftShoulder/LeftArm/LeftForeArm/LeftHand/Leftball";
    public const string NODE_PATH_RIGHT_BALL = "Root/Bip001/Hips/Spine/Spine1/Chest/RightShoulder/RightArm/RightForeArm/RightHand/Rightball";

    public static void Sample(GameObject go, AnimationClip[] clips)
    {
        string filename = "Assets/Resources/Data/Dribble/" + go.name + ".asset";
        DribbleData info = AssetDatabase.LoadAssetAtPath<DribbleData>(filename);
        if (info == null)
        {
            info = ScriptableObject.CreateInstance<DribbleData>();
            AssetDatabase.CreateAsset(info, filename);
        }
        info.Infos.Clear();

        foreach (var clip in clips)
        {
            Debug.LogFormat("导出运球数据，{0} {1}", go.name, clip.name);
            var clipInfo = Sample(go, clip);
            info.Infos.Add(clipInfo);
        }

        EditorUtility.SetDirty(info);
    }

    public static DribbleData.Clip Sample(GameObject go, AnimationClip clip)
    {
        Transform leftBall = go.transform.Find(NODE_PATH_LEFT_BALL);
        if (leftBall == null)
        {
            Debug.LogErrorFormat("GameObject: {0} has no bone Leftball.", go.name);
            return null;
        }
        Transform rightBall = go.transform.Find(NODE_PATH_RIGHT_BALL);
        if (rightBall == null)
        {
            Debug.LogErrorFormat("GameObject: {0} has no bone Rightball.", go.name);
            return null;
        }

        DribbleData.Clip clipInfo = new DribbleData.Clip()
        {
            ClipName = clip.name,
            NameHash = Animator.StringToHash(clip.name),
        };
        DribbleData.Entry curEventInfo = null;
        foreach (var evt in clip.events)
        {
            if (evt.functionName == "DribbleRelease")
            {
                AnimationMode.BeginSampling();
                AnimationMode.SampleAnimationClip(go, clip, evt.time);
                AnimationMode.EndSampling();

                DribbleType type;
                Hand hand;
                if (!DribbleSimulator.TryParseReleaseInfo(evt.stringParameter, out type, out hand))
                {
                    Debug.LogErrorFormat("错误的DribbleRelease参数: {0} GO: {1} Clip: {2}",
                        evt.stringParameter, go.name, clipInfo.ClipName);
                    return null;
                }
                Vector3 releasePos = (hand == Hand.Left ? leftBall.position : rightBall.position);
                curEventInfo = new DribbleData.Entry()
                {
                    ReleaseTime = evt.time,
                    ReleaseNormalizedTime = evt.time / clip.length,
                    Type = type,
                    ReleaseHand = hand,
                    ReleasePosition = releasePos,
                };
            }
            else if (evt.functionName == "DribbleRegain")
            {
                AnimationMode.BeginSampling();
                AnimationMode.SampleAnimationClip(go, clip, evt.time);
                AnimationMode.EndSampling();

                if (curEventInfo == null)
                {
                    Debug.LogErrorFormat("Release与Regain不匹配, GameObject:{0} clip:{1}", go.name, clip.name);
                    return null;
                }
                Hand hand;
                DribbleSimulator.TryParseRegainInfo(evt.stringParameter, out hand);
                Vector3 regainPos = (hand == Hand.Left ? leftBall.position : rightBall.position);
                curEventInfo.RegainTime = evt.time;
                curEventInfo.RegainNormalizedTime = evt.time / clip.length;
                curEventInfo.RegainHand = hand;
                curEventInfo.RegainPosition = regainPos;
                clipInfo.Events.Add(curEventInfo);
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
