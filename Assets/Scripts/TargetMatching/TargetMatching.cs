using UnityEngine;

[RequireComponent(typeof(Animator))]
public class TargetMatching : MonoBehaviour
{
    public Vector3 Scale { get; private set; }

    TargetMatchingData mStdData;
    TargetMatchingData mSelfData;
    Animator mAnimator;

    void Awake()
    {
        mAnimator = GetComponent<Animator>();
    }

    void Start()
    {
        mStdData = Resources.Load<TargetMatchingData>("Data/TargetMatching/Char1_Thin");
        Debug.Assert(mStdData != null);
        mSelfData = Resources.Load<TargetMatchingData>("Data/TargetMatching/" + name);
        Debug.Assert(mSelfData != null);
    }

    void StartMatching()
    {
        if (!enabled)
            return;
        AnimatorStateInfo curInfo = mAnimator.GetCurrentAnimatorStateInfo(0);
        var clipMatchingData = mStdData.Clips.Find(i => i.NameHash == curInfo.shortNameHash);
        Debug.AssertFormat(clipMatchingData != null, "Can't find target matching clip info");

        var entry = clipMatchingData.Entries.Find(e => Mathf.Abs(e.StartNormalizedTime - curInfo.normalizedTime) < 0.01f);
        Debug.Assert(entry != null);

        Vector3 movement = entry.TargetPosition - entry.StartPosition;
        Vector3 dir = movement.normalized;
        float dist = movement.magnitude;
        Vector3 targetPosition = transform.position + transform.TransformDirection(dir) * dist;
        mAnimator.MatchTarget(targetPosition, Quaternion.identity, entry.TargetBodyPart,
            new MatchTargetWeightMask(entry.PositionWeight, 0f), entry.StartNormalizedTime, entry.TargetNormalizedTime);
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = "TargetPosition";
        go.transform.position = targetPosition;
        go.GetComponent<Renderer>().material.color = Color.red;
        go.transform.localScale = Vector3.one / 10;
    }

    public Vector3 GetAdjust(int clipNameHash, float normalizedTime)
    {
        if (!enabled)
            return Vector3.zero;
        var stdClipData = mStdData.Clips.Find(c => c.NameHash == clipNameHash);
        var selfClipData = mSelfData.Clips.Find(c => c.NameHash == clipNameHash);
        var stdEntry = stdClipData.Entries.Find(e => normalizedTime > e.StartNormalizedTime);
        var selfEntry = selfClipData.Entries.Find(e => normalizedTime > e.StartNormalizedTime);
        Vector3 totalAdjust = stdEntry.TargetPosition - Vector3.Scale(selfEntry.TargetPosition, transform.localScale);
        float t = (normalizedTime - stdEntry.StartNormalizedTime) / 
            (stdEntry.TargetNormalizedTime - stdEntry.StartNormalizedTime);
        Vector3 adjust = Vector3.Scale(totalAdjust * t, stdEntry.PositionWeight);
        return adjust;
    }
}
