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

    public const float BALL_RADIUS = 0.123f;
    public const float GRAVITY = -9.8f;

    public BodyType BodyType { get { return mBodyType; } }
    BodyType mBodyType;
    Animator mAnimator;
    Transform[] mHands = new Transform[(int)Hand.Max];
    Transform mBall = null;
    DribbleData mData;
    DribbleData.Entry mCurDataEntry;
    DribbleData.Entry mCurDataEntry1;
    AnimatorStateInfo mDribblingStateInfo;
    AnimCtrlData mControllerData = null;

    Vector3 mVelocity;

    void Awake()
    {
        mAnimator = GetComponent<Animator>();
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
        string controllerDataPath = string.Format("Data/{0}.controller", mAnimator.runtimeAnimatorController.name);
        mControllerData = DataResources.Load<AnimCtrlData>(controllerDataPath);
        Debug.Log(mControllerData);
    }

    void Update()
    {
        var curStateInfo = mAnimator.GetCurrentAnimatorStateInfo(0);
        var nextStateInfo = mAnimator.GetNextAnimatorStateInfo(0);
        if (State == DribbleState.Dribbling)
        {
            if (curStateInfo.shortNameHash == mDribblingStateInfo.shortNameHash ||
                nextStateInfo.shortNameHash == mDribblingStateInfo.shortNameHash)
            {
                Vector3 position = mBall.transform.localPosition;
                position += mVelocity * Time.deltaTime;
                mBall.transform.localPosition = position;
                mVelocity.y += GRAVITY * Time.deltaTime;
            }
            else
                State = DribbleState.InHand;
        }
    }

    void LateUpdate()
    {
        var curStateInfo = mAnimator.GetCurrentAnimatorStateInfo(0);
        var nextStateInfo = mAnimator.GetNextAnimatorStateInfo(0);
        ProcessStateInfo(curStateInfo);
        ProcessStateInfo(nextStateInfo);

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
            mBall.localScale = Vector3.one * BALL_RADIUS * 2 / transform.localScale.y;
            //mBall.SetParent(null, true);
            //mBall.localScale = Vector3.one / 5;
        }
        else if (mState == DribbleState.InHand)
        {
            Debug.AssertFormat(mBall != null, "No ball when setting dribble state to InHand.");
            mBall.SetParent(mHands[(int)Hand], false);
            mBall.localScale = Vector3.one * BALL_RADIUS * 2 / transform.localScale.y;
            mBall.localPosition = Vector3.zero;
        }
    }

    void ProcessStateInfo(AnimatorStateInfo stateInfo)
    {
        if (stateInfo.shortNameHash == 0)
            return;
        float normalizedTime = stateInfo.normalizedTime;
        if (stateInfo.loop)
            normalizedTime = Mathf.Repeat(normalizedTime, 1);
        var blendTree = mControllerData.GetBlendTree(stateInfo.shortNameHash);
        if (blendTree != null)
        {
            float t;
            AnimCtrlData.ChildMotion motion0, motion1;
            if (!blendTree.GetBlendInfo(mAnimator, out t, out motion0, out motion1))
            {
                Debug.LogError("GetBlendTreeInfo error.");
                return;
            }
            var clipData0 = mData.GetClipData(motion0.Motion.NameHash);
            Debug.AssertFormat(clipData0 != null, "Can't find clip data. Motion:{0}({1}) Data:{2}",
                motion0.Motion.Name, motion0.Motion.NameHash, mData);
            var clipData1 = mData.GetClipData(motion1.Motion.NameHash);
            Debug.AssertFormat(clipData0 != null, "Can't find clip data. Motion:{0}({1}) Data:{2}",
                motion1.Motion.Name, motion1.Motion.NameHash, mData);
            Debug.Assert(clipData0.Entries.Count == clipData1.Entries.Count);
            if (State == DribbleState.InHand)
            {
                DribbleData.Entry entry0 = null, entry1 = null;
                for (int i = 0; i < clipData0.Entries.Count; ++i)
                {
                    if (clipData0.Entries[i].InPeriod(normalizedTime) ||
                        clipData1.Entries[i].InPeriod(normalizedTime))
                    {
                        entry0 = clipData0.Entries[i];
                        entry1 = clipData1.Entries[i];
                        break;
                    }
                }
                if (entry0 != null && entry1 != null)
                {
                    Out(stateInfo, normalizedTime, t, entry0, entry1);
                }
            }
            else if (State == DribbleState.Dribbling)
            {
                Debug.Assert(mCurDataEntry != null && mCurDataEntry1 != null);
                if (stateInfo.shortNameHash == mDribblingStateInfo.shortNameHash)
                {
                    if (mVelocity.y < 0)
                    {
                        float boundNormalizedTime = Mathf.Lerp(mCurDataEntry.BounceNormalizedTime, mCurDataEntry1.BounceNormalizedTime, t);
                        if (normalizedTime >= boundNormalizedTime)
                            Bounce(stateInfo, normalizedTime, t, mCurDataEntry, mCurDataEntry1);
                    }
                    else if (mVelocity.y > 0)
                    {
                        float inNormalizedTime = Mathf.Lerp(mCurDataEntry.InNormalizedTime, mCurDataEntry1.InNormalizedTime, t);
                        if (normalizedTime >= inNormalizedTime)
                            In(normalizedTime, t, mCurDataEntry, mCurDataEntry1);
                    }
                    else
                        Debug.Assert(false, "Horizontal velocity can't be 0");
                }
            }
        }
        else
        {
            if (State == DribbleState.InHand)
            {
                var clipData = mData.GetClipData(stateInfo.shortNameHash);
                if (clipData != null)
                {
                    var entry = clipData.GetEntry(normalizedTime);
                    if (entry != null)
                    {
                        Out(stateInfo, normalizedTime, entry);
                    }
                }
            }
            else if (State == DribbleState.Dribbling)
            {
                Debug.Assert(mCurDataEntry != null, "No current dribble data entry.");
                if (stateInfo.shortNameHash == mDribblingStateInfo.shortNameHash)
                {
                    if (mVelocity.y < 0)
                    {
                        if (normalizedTime >= mCurDataEntry.BounceNormalizedTime)
                            Bounce(stateInfo, normalizedTime, mCurDataEntry);
                    }
                    else if (mVelocity.y > 0)
                    {
                        if (normalizedTime >= mCurDataEntry.InNormalizedTime)
                            In();
                    }
                    else
                        Debug.Assert(false, "Horizontal velocity can't be 0");
                }
            }
        }
    }

    void Out(AnimatorStateInfo stateInfo, float normalizedTime, DribbleData.Entry entry)
    {
        float playSpeed = stateInfo.speed * stateInfo.speedMultiplier;
        float stateLength = stateInfo.length * playSpeed;
        float outInTime = (entry.InNormalizedTime * stateLength - normalizedTime * stateLength) / playSpeed;
        float outBounceTime = (entry.BounceNormalizedTime * stateLength - normalizedTime * stateLength) / playSpeed;
        Vector3 outPos = entry.OutPosition[(int)BodyType];
        Vector3 inPos = entry.InPosition[(int)BodyType];
        mVelocity = CalcOutVelocity(outPos, inPos, outInTime, outBounceTime);
        State = DribbleState.Dribbling;
        mDribblingStateInfo = stateInfo;
        mCurDataEntry = entry;

        //Utils.DrawPoint("OutPos", transform.TransformPoint(outPos), Color.red);
        //Utils.DrawPoint("OutPos", outPos, Color.red);
        //Debug.Break();
    }

    void Out(AnimatorStateInfo stateInfo, float normalizedTime, float t,
        DribbleData.Entry entry0, DribbleData.Entry entry1)
    {
        float outNormalizedTime = Mathf.Lerp(entry0.OutNormalizedTime, entry1.OutNormalizedTime, t);
        float bounceNormalizedTime = Mathf.Lerp(entry0.BounceNormalizedTime, entry1.BounceNormalizedTime, t);
        float inNormalizedTime = Mathf.Lerp(entry0.InNormalizedTime, entry1.InNormalizedTime, t);
        if (normalizedTime < outNormalizedTime || normalizedTime > inNormalizedTime)
            return;

        float outInTime = (inNormalizedTime - normalizedTime) * stateInfo.length;
        float outBounceTime = (bounceNormalizedTime - normalizedTime) * stateInfo.length;

        Vector3 outPos = Vector3.Lerp(entry0.OutPosition[(int)BodyType], entry1.OutPosition[(int)BodyType], t);
        Vector3 inPos = Vector3.Lerp(entry0.InPosition[(int)BodyType], entry1.InPosition[(int)BodyType], t);
        mVelocity = CalcOutVelocity(outPos, inPos, outInTime, outBounceTime);
        State = DribbleState.Dribbling;
        mDribblingStateInfo = stateInfo;
        mCurDataEntry = entry0;
        mCurDataEntry1 = entry1;

        //Debug.LogFormat("DribbleOut, NTime:{0} outNTime:{1}", normalizedTime, outNormalizedTime);
        //Utils.DrawPoint("OutPos", transform.TransformPoint(outPos), Color.red);
        //Utils.DrawPoint("InPos", transform.TransformPoint(inPos), Color.blue);
        ////Utils.DrawPoint("OutPos", outPos, Color.red);
        //Debug.Break();
    }

    Vector3 CalcOutVelocity(Vector3 outPos, Vector3 inPos, float outInTime, float outBounceTime)
    {
        Vector3 velocity = (inPos - outPos) / outInTime;
        float outHeight = (BALL_RADIUS - outPos.y);
        float vh = (outHeight / outBounceTime - GRAVITY * outBounceTime / 2);
        velocity.y = vh;
        return velocity;
    }

    void Bounce(AnimatorStateInfo stateInfo, float normalizedTime, DribbleData.Entry entry)
    {
        float stateLength = stateInfo.length;
        float bounceInTime = (entry.InNormalizedTime - normalizedTime) * stateLength;
        Vector3 inPos = entry.InPosition[(int)BodyType];
        CalcInVelocity(mBall.transform.localPosition, inPos, bounceInTime);
    }

    void Bounce(AnimatorStateInfo stateInfo, float normalizedTime, float t,
        DribbleData.Entry entry0, DribbleData.Entry entry1)
    {
        float inNormalizedTime = Mathf.Lerp(entry0.InNormalizedTime, entry1.InNormalizedTime, t);
        float bounceInTime = (inNormalizedTime - normalizedTime) * stateInfo.length;

        Vector3 inPos = Vector3.Lerp(entry0.InPosition[(int)BodyType], entry1.InPosition[(int)BodyType], t);

        CalcInVelocity(mBall.transform.localPosition, inPos, bounceInTime);
    }

    void CalcInVelocity(Vector3 startPos, Vector3 inPos, float bounceInTime)
    {
        float inHeight = (inPos.y - startPos.y);
        float vh = (inHeight / bounceInTime - GRAVITY * bounceInTime / 2);
        mVelocity.y = vh;
    }

    void In()
    {
        //Utils.DrawPoint("InPos", transform.TransformPoint(mCurDataEntry.InPosition[(int)BodyType]), Color.blue);
        //Utils.DrawPoint("InPos", mInPos, Color.blue);
        //Debug.Break();

        Hand hand = mCurDataEntry.InHand;
        State = DribbleState.InHand;
        Hand = hand;
        mDribblingStateInfo = new AnimatorStateInfo();
        mCurDataEntry = null;
    }

    void In(float normalizedTime, float t, DribbleData.Entry entry0, DribbleData.Entry entry1)
    {
        //Debug.LogFormat("DribbleIn, NTime:{0} inNTime:{1}", normalizedTime, inNormalizedTime);
        //Utils.DrawPoint("InPos", transform.TransformPoint(mCurDataEntry.InPosition[(int)BodyType]), Color.blue);
        //Utils.DrawPoint("InPos", mInPos, Color.blue);
        //Debug.Break();

        Debug.Assert(entry0.InHand == entry1.InHand);
        Hand hand = entry0.InHand;
        State = DribbleState.InHand;
        Hand = hand;
        mDribblingStateInfo = new AnimatorStateInfo();
        mCurDataEntry = null;
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
