using System;
using UnityEngine;

public enum DribbleType
{
    Ground,
    Direct,
}

public enum Hand
{
    Left,
    Right,
    Both,
    Max,
}

public enum DribbleState
{
    None,
    InHand,
    Dribbling,
}

[RequireComponent(typeof(Animator))]
public class DribbleSimulator : MonoBehaviour
{
    public DribbleState State
    {
        get { return mState; }
        set
        {
            if (mState != value)
            {
                mState = value;
                mReattachBall = true;
            }
        }
    }
    DribbleState mState;

    public Hand Hand
    {
        get { return mHand; }
        set
        {
            if (mHand != value)
            {
                mHand = value;
                mReattachBall = true;
            }
        }
    }
    Hand mHand;

    bool mReattachBall = false;

    BodyType mBodyType;
    Animator mAnimator;
    TargetMatching mTargetMatching;
    Transform[] mHands = new Transform[(int)Hand.Max];
    Transform mBall = null;
    DribbleData mData;
    DribbleData.Entry mCurDataEntry;

    Vector3 mVelocity;
    float mPlaySpeed = 1f;
    [HideInInspector]
    public bool mMirror = false;

    void Awake()
    {
        mAnimator = GetComponent<Animator>();
        mTargetMatching = GetComponent<TargetMatching>();
        mBodyType = (BodyType)Enum.Parse(BodyType.Count.GetType(), name);
        mHands[(int)Hand.Left] = transform.Find("Root/Bip001/Hips/Spine/Spine1/Chest/LeftShoulder/LeftArm/LeftForeArm/LeftHand/Leftball");
        mHands[(int)Hand.Right] = transform.Find("Root/Bip001/Hips/Spine/Spine1/Chest/RightShoulder/RightArm/RightForeArm/RightHand/Rightball");
        mHands[(int)Hand.Both] = transform.Find("Root/Ball");
        GameObject goBall = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        goBall.name = "Ball";
        goBall.GetComponent<Renderer>().material.color = Color.red;
        mBall = goBall.transform;
    }

    void Start()
    {
        mData = Resources.Load<DribbleData>("Data/Dribble");
    }

    void Update()
    {
        if (mReattachBall)
        {
            ReattachBall();
            mReattachBall = false;
        }

        if (State == DribbleState.Dribbling)
        {
            Vector3 position = mBall.transform.position;
            position += mVelocity * Time.deltaTime * mPlaySpeed;
            if (position.y <= 0)
            {
                position.y = -position.y;
                mVelocity.y = -mVelocity.y;
            }
            mBall.transform.position = position;
        }
    }

    void ReattachBall()
    {
        if (mState == DribbleState.None)
        {
            if (mBall != null)
            {
                mBall.SetParent(null, true);
            }
        }
        else if (mState == DribbleState.Dribbling)
        {
            mBall.SetParent(null, true);
            mBall.localScale = Vector3.one / 5;
        }
        else if (mState == DribbleState.InHand)
        {
            Debug.AssertFormat(mBall != null, "No ball when setting dribble state to InHand.");
            mBall.SetParent(mHands[(int)Hand], false);
            mBall.localScale = Vector3.one / 5 / transform.localScale.y;
            mBall.localPosition = Vector3.zero;
        }
    }

    void DribbleOut(string args)
    {
        AnimatorStateInfo stateInfo = mAnimator.GetCurrentAnimatorStateInfo(0);

        var clipInfo = mData.Clips.Find(ci => ci.NameHash == stateInfo.shortNameHash);
        Debug.AssertFormat(clipInfo != null, "Can't find dribble clip info");

        float normalizedTime = stateInfo.normalizedTime;
        if (stateInfo.loop)
            normalizedTime = Mathf.Repeat(stateInfo.normalizedTime, 1);

        //Debug.LogFormat("DribbleOut, {0} {1} NorTime:{2} PlaySpeed:{3} \nClipInfo:{4}",
        //    type, hand, normalizedTime, playSpeed, clipInfo);

        var entry = clipInfo.Entries.Find(ei => Mathf.Abs(normalizedTime - ei.OutNormalizedTime) < 0.01f);
        if (entry == null)
        {
            stateInfo = mAnimator.GetNextAnimatorStateInfo(0);
            clipInfo = mData.Clips.Find(ci => ci.NameHash == stateInfo.shortNameHash);
            Debug.AssertFormat(clipInfo != null, "Can't find dribble clip info");
            normalizedTime = stateInfo.normalizedTime;
            if (stateInfo.loop)
                normalizedTime = Mathf.Repeat(stateInfo.normalizedTime, 1);
            entry = clipInfo.Entries.Find(ei => Mathf.Abs(normalizedTime - ei.OutNormalizedTime) < 0.01f);
        }
        Debug.AssertFormat(entry != null, "DibbleOut, entry not found. NormalizedTime:{0} Clips:\n{1}",
            normalizedTime, clipInfo);

        float playSpeed = stateInfo.speed * stateInfo.speedMultiplier;

        Vector3 targetMatchingAdjustOut = Vector3.zero;
        Vector3 targetMatchingAdjustIn = Vector3.zero;
        if (mTargetMatching != null)
        {
            targetMatchingAdjustOut = mTargetMatching.GetAdjust(stateInfo.shortNameHash, entry.OutNormalizedTime);
            targetMatchingAdjustIn = mTargetMatching.GetAdjust(stateInfo.shortNameHash, entry.InNormalizedTime);
        }

        float time = (entry.InTime - entry.OutTime);
        Vector3 outPos = entry.OutPosition[(int)mBodyType] + targetMatchingAdjustOut;
        Vector3 inPos = entry.InPosition[(int)mBodyType] + targetMatchingAdjustIn;
        if (mMirror)
        {
            outPos.x = -outPos.x;
            inPos.x = -inPos.x;
        }
        outPos = transform.TransformPoint(outPos);
        inPos = transform.TransformPoint(inPos);
        mVelocity = (inPos - outPos) / time;
        mVelocity.y = -(inPos.y + outPos.y) / time;
        State = DribbleState.Dribbling;
        mCurDataEntry = entry;
        mPlaySpeed = playSpeed;
    }

    void DribbleIn(string args)
    {
        Debug.Assert(State == DribbleState.Dribbling);
        Debug.Assert(mCurDataEntry != null);

        Hand hand = mCurDataEntry.InHand;
        State = DribbleState.InHand;
        if (mMirror)
            Hand = MirrorHand(hand);
        else
            Hand = hand;
    }

    public static bool TryParseOutInfo(string args, out DribbleType type, out Hand handSide)
    {
        try
        {
            string[] tokens = args.Split(',');
            type = (DribbleType)(int.Parse(tokens[0]));
            handSide = (Hand)Enum.Parse(Hand.Max.GetType(), tokens[1]);
            return true;
        }
        catch
        {
            type = DribbleType.Direct;
            handSide = Hand.Max;
            return false;
        }
    }

    public static bool TryParseInInfo(string args, out Hand handSide)
    {
        try
        {
            handSide = (Hand)Enum.Parse(Hand.Max.GetType(), args);
            return true;
        }
        catch
        {
            handSide = Hand.Max;
            return false;
        }
    }

    public static Hand MirrorHand(Hand handSide)
    {
        switch (handSide)
        {
            case Hand.Left:
                return Hand.Right;
            case Hand.Right:
                return Hand.Left;
            case Hand.Both:
                return Hand.Both;
            default:
                return Hand.Max;
        }
    }
}
