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
    DribbleData.Entry mCurEntry;
    Transform[] mBallNodes = new Transform[(int)Hand.Max];

    void Awake()
    {
        mAnimator = GetComponent<Animator>();
        GetComponent<RuntimeSampleController>().Samplers.Add(this);
        mBallNodes[(int)Hand.Left] = transform.Find(Utils.NODE_PATH_LEFT_BALL);
        mBallNodes[(int)Hand.Right] = transform.Find(Utils.NODE_PATH_RIGHT_BALL);
    }

    void IRuntimeSampler.OnStartClip(AnimationClip clip)
    {
        mCurClipData = Data.GetClipData(clip.name);
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
        var entry = mCurClipData.GetEntry(curStateInfo.normalizedTime, 0.02f);
        Debug.AssertFormat(entry != null, "DribbleSimRuntimeSampler.DribbleOut: Can't find entry.\n" +
            "Clip:{0} State:{1} {2}", mCurClipData, curStateInfo.shortNameHash, curStateInfo.normalizedTime);
        Transform node = mBallNodes[(int)entry.OutHand];
        Vector3 pos = transform.InverseTransformPoint(node.position);
        entry.OutPosition[(int)BodyType] = pos;
        mCurEntry = entry;
#if DEBUG_SAMPLE
        Utils.DrawPoint("OutPos" + BodyType + mCurClipData.ClipName, node.position, Color.red);
        Debug.LogFormat("OutPos:{0} NorTime:{1} {2}", node.position, curStateInfo.normalizedTime, entry.OutNormalizedTime);
        Debug.Break();
#endif
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
        Debug.Assert(mCurEntry != null);
        Debug.Assert(Mathf.Abs(curStateInfo.normalizedTime - mCurEntry.InNormalizedTime) < 0.02f);
        Transform node = mBallNodes[(int)mCurEntry.InHand];
        Vector3 pos = transform.InverseTransformPoint(node.position);
        mCurEntry.InPosition[(int)BodyType] = pos;
#if DEBUG_SAMPLE
        Utils.DrawPoint("InPos" + BodyType + mCurClipData.ClipName, node.position, Color.blue);
        Debug.LogFormat("InPos:{0}", node.position);
        Debug.Break();
#endif
    }

    void StartMatching()
    {
    }

    void EndMatching()
    {
    }
}
