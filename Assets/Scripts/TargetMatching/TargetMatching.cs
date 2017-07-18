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
        Debug.Assert(mData != null, "Can't load target matching data.");
    }

    void StartMatching()
    {
        if (!enabled)
            return;
        AnimatorStateInfo curStateInfo = mAnimator.GetCurrentAnimatorStateInfo(0);
        AnimatorStateInfo nextStateInfo = mAnimator.GetNextAnimatorStateInfo(0);
        if (mAnimator.IsInTransition(0))
        {
            Debug.LogErrorFormat("Can't start target matching while in transition.\n" +
                "CurState:{0} {1} NextState:{2} {3} Data:\n{4}",
                curStateInfo.shortNameHash, curStateInfo.normalizedTime,
                nextStateInfo.shortNameHash, nextStateInfo.normalizedTime, mData);
            return;
        }
        
        AnimatorStateInfo stateInfo = curStateInfo;
        var clipData = mData.Clips.Find(c => c.NameHash == stateInfo.shortNameHash);
        Debug.AssertFormat(clipData != null, "Can't find target matching clip info. NameHash:{0}\n{1}",
            stateInfo.shortNameHash, mData);

        var entry = clipData.Entries.Find(e => Mathf.Abs(e.StartNormalizedTime - stateInfo.normalizedTime) < 0.01f);
        Debug.AssertFormat(entry != null, "Can't find entry. NorTime:{0} Data:{1}", stateInfo.normalizedTime, clipData);

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
        Vector3 adjust = Vector3.zero;
        if (enabled)
        {
            var clipData = mData.Clips.Find(c => c.NameHash == clipNameHash);
            if (clipData != null)
            {
                var entry = clipData.Entries.Find(e => normalizedTime > e.StartNormalizedTime);
                if (entry != null)
                {
                    Vector3 totalAdjust = entry.TargetPosition[(int)BodyType.Char1_Thin] -
                        Vector3.Scale(entry.TargetPosition[(int)mBodyType], transform.localScale);
                    float t = (normalizedTime - entry.StartNormalizedTime) /
                        (entry.TargetNormalizedTime - entry.StartNormalizedTime);
                    adjust = Vector3.Scale(totalAdjust * t, entry.PositionWeight);
                }
            }
        }
        return adjust;
    }
}
