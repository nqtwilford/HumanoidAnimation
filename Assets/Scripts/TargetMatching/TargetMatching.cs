using UnityEngine;

[RequireComponent(typeof(Animator))]
public class TargetMatching : MonoBehaviour
{
    public Vector3 Scale { get; private set; }

    TargetMatchingData mData;
    Animator mAnimator;
    BodyType mBodyType;

    void Awake()
    {
        mAnimator = GetComponent<Animator>();
        mBodyType = (BodyType)System.Enum.Parse(BodyType.Count.GetType(), name);
    }

    void Start()
    {
        mData = Resources.Load<TargetMatchingData>("Data/TargetMatching");
        Debug.Assert(mData != null);
    }

    void StartMatching()
    {
        if (!enabled)
            return;
        AnimatorStateInfo curInfo = mAnimator.GetCurrentAnimatorStateInfo(0);
        var clipMatchingData = mData.Clips.Find(i => i.NameHash == curInfo.shortNameHash);
        Debug.AssertFormat(clipMatchingData != null, "Can't find target matching clip info");

        var entry = clipMatchingData.Entries.Find(e => Mathf.Abs(e.StartNormalizedTime - curInfo.normalizedTime) < 0.01f);
        Debug.Assert(entry != null);

        Vector3 movement = entry.TargetPosition[(int)BodyType.Char1_Thin] - entry.StartPosition[(int)BodyType.Char1_Thin];
        Vector3 dir = movement.normalized;
        float dist = movement.magnitude;
        Vector3 targetPosition = transform.position + transform.TransformDirection(dir) * dist;
        mAnimator.MatchTarget(targetPosition, Quaternion.identity, entry.TargetBodyPart,
            new MatchTargetWeightMask(entry.PositionWeight, 0f), entry.StartNormalizedTime, entry.TargetNormalizedTime);
        /*
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = "TargetPosition";
        go.transform.position = targetPosition;
        go.GetComponent<Renderer>().material.color = Color.red;
        go.transform.localScale = Vector3.one / 10;
        */
    }

    public Vector3 GetAdjust(int clipNameHash, float normalizedTime)
    {
        if (!enabled)
            return Vector3.zero;
        var clipData = mData.Clips.Find(c => c.NameHash == clipNameHash);
        var entry = clipData.Entries.Find(e => normalizedTime > e.StartNormalizedTime);
        Vector3 totalAdjust = entry.TargetPosition[(int)BodyType.Char1_Thin] -
            Vector3.Scale(entry.TargetPosition[(int)mBodyType], transform.localScale);
        float t = (normalizedTime - entry.StartNormalizedTime) / 
            (entry.TargetNormalizedTime - entry.StartNormalizedTime);
        Vector3 adjust = Vector3.Scale(totalAdjust * t, entry.PositionWeight);
        return adjust;
    }
}
