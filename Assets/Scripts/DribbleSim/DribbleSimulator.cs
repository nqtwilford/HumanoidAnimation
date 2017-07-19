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
                //Debug.LogFormat("Set dribble state, {0}", value);
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

    public BodyType BodyType { get { return mBodyType; } }
    BodyType mBodyType;
    Animator mAnimator;
    //TargetMatching mTargetMatching;
    Transform[] mHands = new Transform[(int)Hand.Max];
    Transform mBall = null;
    DribbleData mData;
    DribbleData.Entry mCurDataEntry;
    int mDribblingClipNameHash;

    Vector3 mVelocity;
    [HideInInspector]
    public bool mMirror = false;

    void Awake()
    {
        mAnimator = GetComponent<Animator>();
        //mTargetMatching = GetComponent<TargetMatching>();
        mBodyType = (BodyType)Enum.Parse(BodyType.Count.GetType(), name);
        mHands[(int)Hand.Left] = transform.Find(Utils.NODE_PATH_LEFT_BALL);
        mHands[(int)Hand.Right] = transform.Find(Utils.NODE_PATH_RIGHT_BALL);
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
        if (State == DribbleState.Dribbling)
        {
            var curStateInfo = mAnimator.GetCurrentAnimatorStateInfo(0);
            var nextStateInfo = mAnimator.GetNextAnimatorStateInfo(0);
            if (curStateInfo.shortNameHash == mDribblingClipNameHash ||
                nextStateInfo.shortNameHash == mDribblingClipNameHash)
            {
                float playSpeed = 1;
                if (curStateInfo.shortNameHash == mDribblingClipNameHash)
                    playSpeed = curStateInfo.speed * curStateInfo.speedMultiplier;
                else if (nextStateInfo.shortNameHash == mDribblingClipNameHash)
                    playSpeed = nextStateInfo.speed * nextStateInfo.speedMultiplier; 

                Vector3 position = mBall.transform.localPosition;
                position += mVelocity * Time.deltaTime * playSpeed;
                if (position.y <= 0)
                {
                    position.y = -position.y;
                    mVelocity.y = -mVelocity.y;
                }
                mBall.transform.localPosition = position;
            }
            else
                State = DribbleState.InHand;
        }
    }

    void LateUpdate()
    {
        if (mReattachBall)
        {
            ReattachBall();
            mReattachBall = false;
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
            mBall.SetParent(transform, true);
            mBall.localScale = Vector3.one / 5 / transform.localScale.y * 1.25f;
            //mBall.SetParent(null, true);
            //mBall.localScale = Vector3.one / 5;
        }
        else if (mState == DribbleState.InHand)
        {
            Debug.AssertFormat(mBall != null, "No ball when setting dribble state to InHand.");
            mBall.SetParent(mHands[(int)Hand], false);
            mBall.localScale = Vector3.one / 5 / transform.localScale.y * 1.25f;
            mBall.localPosition = Vector3.zero;
        }
    }

    void DribbleOut(string args)
    {
        AnimatorStateInfo curStateInfo = mAnimator.GetCurrentAnimatorStateInfo(0);
        AnimatorStateInfo nextStateInfo = mAnimator.GetNextAnimatorStateInfo(0);
        AnimatorStateInfo stateInfo = curStateInfo;
        var entry = GetEntry(stateInfo);
        if (entry == null)
        {
            stateInfo = nextStateInfo;
            entry = GetEntry(stateInfo);
        }
        Debug.AssertFormat(entry != null, "DibbleOut, entry not found. CurState:{0} {1} NextState:{2} {3} Data:\n{4}",
            curStateInfo.shortNameHash, curStateInfo.normalizedTime, 
            nextStateInfo.shortNameHash, nextStateInfo.normalizedTime, mData);

        //Vector3 targetMatchingAdjustOut = Vector3.zero;
        //Vector3 targetMatchingAdjustIn = Vector3.zero;
        //if (mTargetMatching != null)
        //{
        //    targetMatchingAdjustOut = mTargetMatching.GetAdjust(stateInfo.shortNameHash, entry.OutNormalizedTime);
        //    targetMatchingAdjustIn = mTargetMatching.GetAdjust(stateInfo.shortNameHash, entry.InNormalizedTime);
        //}

        float normalizedTime = stateInfo.normalizedTime;
        if (stateInfo.loop)
            normalizedTime = Mathf.Repeat(stateInfo.normalizedTime, 1);
        float time = (entry.InTime - normalizedTime);
        Vector3 outPos = entry.OutPosition[(int)mBodyType];// + targetMatchingAdjustOut;
        Vector3 inPos = entry.InPosition[(int)mBodyType];// + targetMatchingAdjustIn;
        if (mMirror)
        {
            outPos.x = -outPos.x;
            inPos.x = -inPos.x;
        }
        //outPos = transform.TransformPoint(outPos);
        //inPos = transform.TransformPoint(inPos);
        mVelocity = (inPos - outPos) / time;
        mVelocity.y = -(inPos.y + outPos.y) / time;
        State = DribbleState.Dribbling;
        mCurDataEntry = entry;
        mDribblingClipNameHash = stateInfo.shortNameHash;

        //Utils.DrawPoint("OutPos", transform.TransformPoint(outPos), Color.red);
        ////Utils.DrawPoint("OutPos", outPos, Color.red);
        //Debug.Break();
        //Debug.LogFormat("DribbleOut, Cur:{0} {1} Next:{2} {3}",
        //    curStateInfo.shortNameHash, curStateInfo.normalizedTime,
        //    nextStateInfo.shortNameHash, nextStateInfo.normalizedTime);
    }

    void DribbleIn(string args)
    {
        //AnimatorStateInfo curStateInfo = mAnimator.GetCurrentAnimatorStateInfo(0);
        //AnimatorStateInfo nextStateInfo = mAnimator.GetNextAnimatorStateInfo(0);
        //Debug.LogFormat("DribbleIn, Cur:{0} {1} Next:{2} {3}",
        //    curStateInfo.shortNameHash, curStateInfo.normalizedTime,
        //    nextStateInfo.shortNameHash, nextStateInfo.normalizedTime);

        if (State != DribbleState.Dribbling)
        {
            //Debug.LogErrorFormat("Not in dribbling state while dribble in.");
            return;
        }
        Debug.Assert(mCurDataEntry != null, "No current dribble data entry.");

        Hand hand = mCurDataEntry.InHand;
        State = DribbleState.InHand;
        if (mMirror)
            Hand = MirrorHand(hand);
        else
            Hand = hand;

        //Utils.DrawPoint("InPos", transform.TransformPoint(mCurDataEntry.InPosition[(int)BodyType]), Color.blue);
        ////Utils.DrawPoint("InPos", mInPos, Color.blue);
        //Debug.Break();
    }

    DribbleData.Entry GetEntry(AnimatorStateInfo stateInfo)
    {
        if (stateInfo.shortNameHash != 0)
        {
            var clipInfo = mData.GetClipData(stateInfo.shortNameHash);
            if (clipInfo != null)
            {
                float normalizedTime = stateInfo.normalizedTime;
                if (stateInfo.loop)
                    normalizedTime = Mathf.Repeat(stateInfo.normalizedTime, 1);
                var entry = clipInfo.GetEntry(normalizedTime);
                return entry;
            }
        }
        return null;
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
