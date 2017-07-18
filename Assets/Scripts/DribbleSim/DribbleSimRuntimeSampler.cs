using System.Collections;
using UnityEngine;
using UnityEditor;

[RequireComponent(typeof(Animator), typeof(RuntimeSampleController))]
public class DribbleSimRuntimeSampler : MonoBehaviour, IRuntimeSampler
{
    public BodyType BodyType;
    public DribbleData Data;

    Animator mAnimator;
    DribbleData.Clip mCurClipData;
    Transform[] mBallNodes = new Transform[(int)Hand.Max];

    void Awake()
    {
        mAnimator = GetComponent<Animator>();
        GetComponent<RuntimeSampleController>().Samplers.Add(this);
        mBallNodes[(int)Hand.Left] = transform.FindChild(Utils.NODE_PATH_LEFT_BALL);
        mBallNodes[(int)Hand.Right] = transform.FindChild(Utils.NODE_PATH_RIGHT_BALL);
    }

    void IRuntimeSampler.OnStartClip(AnimationClip clip)
    {
        mCurClipData = Data.Clips.Find(c => c.ClipName == clip.name);
        Debug.AssertFormat(mCurClipData != null, "Can't find clip data: {0}", clip.name);
    }

    void IRuntimeSampler.OnFinishClip(AnimationClip clip)
    {
        mCurClipData = null;
    }

    void IRuntimeSampler.OnComplete()
    {
        EditorUtility.SetDirty(Data);
    }

    void DribbleOut()
    {
        StartCoroutine(SampleDribbleOut());
    }

    IEnumerator SampleDribbleOut()
    {
        yield return new WaitForEndOfFrame();
        DoSampleDribbleOut();
    }

    void DoSampleDribbleOut()
    {
        Debug.Assert(mCurClipData != null);
        var curStateInfo = mAnimator.GetCurrentAnimatorStateInfo(0);
        Debug.Assert(curStateInfo.shortNameHash == mCurClipData.NameHash);
        var entry = mCurClipData.Entries.Find(e => Mathf.Abs(curStateInfo.normalizedTime - e.OutNormalizedTime) < 0.02f);
        Debug.AssertFormat(entry != null, "DribbleSimRuntimeSampler.DribbleOut: Can't find entry.\n" +
            "Clip:{0} State:{1} {2}", mCurClipData, curStateInfo.shortNameHash, curStateInfo.normalizedTime);
        Transform node = mBallNodes[(int)entry.OutHand];
        Vector3 pos = transform.InverseTransformPoint(node.position);
        entry.OutPosition[(int)BodyType] = pos;
        //Utils.DrawPoint("OutPos" + BodyType + mCurClipData.ClipName, node.position, Color.red);
        //Debug.LogFormat("OutPos:{0} NorTime:{1} {2}", node.position, curStateInfo.normalizedTime, entry.OutNormalizedTime);
        //Debug.Break();
    }

    void DribbleIn()
    {
        StartCoroutine(SampleDribbleIn());
    }

    IEnumerator SampleDribbleIn()
    {
        yield return new WaitForEndOfFrame();
        DoSampleDribbleIn();
    }

    void DoSampleDribbleIn()
    {
        Debug.Assert(mCurClipData != null);
        var curStateInfo = mAnimator.GetCurrentAnimatorStateInfo(0);
        Debug.Assert(curStateInfo.shortNameHash == mCurClipData.NameHash);
        var entry = mCurClipData.Entries.Find(e => Mathf.Abs(curStateInfo.normalizedTime - e.InNormalizedTime) < 0.02f);
        Debug.Assert(entry != null);
        Transform node = mBallNodes[(int)entry.InHand];
        Vector3 pos = transform.InverseTransformPoint(node.position);
        entry.InPosition[(int)BodyType] = pos;
        //Utils.DrawPoint("InPos" + BodyType + mCurClipData.ClipName, node.position, Color.blue);
        //Debug.LogFormat("InPos:{0}", node.position);
        //Debug.Break();
    }

    void StartMatching()
    {
    }
}
