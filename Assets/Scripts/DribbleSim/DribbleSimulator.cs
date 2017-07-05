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
    DribbleState State
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

    Hand Hand
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

    Animator mAnimator;
    TargetMatching mTargetMatching;
    Transform[] mHands = new Transform[(int)Hand.Max];
    Transform mBall = null;
    DribbleData mDribbleInfo;

    Vector3 mVelocity;
    float mPlaySpeed = 1f;
    bool mMirror = false;
    //Vector3 mLastPos;
    //float mLastNormalizedTime = 0f;
    Vector3 mInitialPos;

    void Awake()
    {
        transform.Find("BallGeo").gameObject.SetActive(false);
        transform.Find("BallGeoL").gameObject.SetActive(false);
        transform.Find("BallGeoR").gameObject.SetActive(false);
        mAnimator = GetComponent<Animator>();
        mTargetMatching = GetComponent<TargetMatching>();
        mHands[(int)Hand.Left] = transform.Find("Root/Bip001/Hips/Spine/Spine1/Chest/LeftShoulder/LeftArm/LeftForeArm/LeftHand/Leftball");
        mHands[(int)Hand.Right] = transform.Find("Root/Bip001/Hips/Spine/Spine1/Chest/RightShoulder/RightArm/RightForeArm/RightHand/Rightball");
        mHands[(int)Hand.Both] = transform.Find("Root/Ball");
        GameObject goBall = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        goBall.name = "Ball";
        goBall.GetComponent<Renderer>().material.color = Color.red;
        mBall = goBall.transform;
        //mLastPos = transform.position;
        mInitialPos = transform.position;
    }

    void Start()
    {
        mDribbleInfo = Resources.Load<DribbleData>("Data/Dribble/" + name);
    }

    void Update()
    {
        if (Input.GetKeyUp(KeyCode.R))
        {
            transform.position = mInitialPos;
            //mLastPos = mInitialPos;
            //mLastNormalizedTime = 0f;
            mAnimator.Play("None", 0, 0);
        }
        if (Input.GetKeyUp(KeyCode.Space))
        {
            State = DribbleState.InHand;
            if (!mMirror)
            {
                Hand = Hand.Left;
                mAnimator.SetBool("mirror", false);
            }
            else
            {
                Hand = Hand.Right;
                mAnimator.SetBool("mirror", true);
            }
            mAnimator.Play("spinmove", 0, 0);
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

        if (mReattachBall)
        {
            ReattachBall();
            mReattachBall = false;
        }

        /*
        var curStateInfo = mAnimator.GetCurrentAnimatorStateInfo(0);
        if (curStateInfo.shortNameHash == Animator.StringToHash("spinmove"))
        {
            if (curStateInfo.normalizedTime - mLastNormalizedTime >= 0.1f && curStateInfo.normalizedTime < 1.1f)
            {
                Vector3 curPos = transform.position;
                Vector3 delta = curPos - mLastPos;
                //Debug.LogFormat("{0} {1} {2}", curStateInfo.normalizedTime, delta.ToString("F3"), delta.magnitude);
                mLastPos = curPos;
                mLastNormalizedTime += 0.1f;
                string name = "Path" + ((int)(mLastNormalizedTime * 10)).ToString();
                GameObject prevGo = GameObject.Find(name);
                if (prevGo != null)
                {
                    Vector3 prevGoPos = prevGo.transform.position;
                    Vector3 deltaCurPos = curPos - mInitialPos;
                    Vector3 deltaPrevGoPos = prevGoPos - mInitialPos;
                    Vector3 ratio = new Vector3(deltaCurPos.x / deltaPrevGoPos.x, deltaCurPos.y / deltaPrevGoPos.y, deltaCurPos.z / deltaPrevGoPos.z);
                    Debug.LogFormat("{0} {1} {2}", curStateInfo.normalizedTime,
                        (curPos - prevGoPos).ToString("F3"), ratio.ToString("F3"));
                }
                GameObject go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                go.name = name;
                go.transform.localScale = Vector3.one / 10;
                go.transform.position = curPos;
                go.transform.GetComponent<Renderer>().material.color = mTargetMatching.enabled ? Color.red : Color.blue;
            }
        }
        */
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
        AnimatorStateInfo curInfo = mAnimator.GetCurrentAnimatorStateInfo(0);
        var clipInfo = mDribbleInfo.Clips.Find(ci => ci.NameHash == curInfo.shortNameHash);
        Debug.AssertFormat(clipInfo != null, "Can't find dribble clip info");

        DribbleType type;
        Hand hand;
        if (!TryParseOutInfo(args, out type, out hand))
        {
            Debug.LogErrorFormat("错误的DribbleOut参数: {0} GO: {1} Clip: {2}", args, name, clipInfo.ClipName);
            return;
        }
        //Debug.LogFormat("DribbleOut, {0} {1} NorTime:{2}", type, handSide, curInfo.normalizedTime);
        float normalizedTime = curInfo.normalizedTime;
        if (curInfo.loop)
            normalizedTime = curInfo.normalizedTime % 1;

        var entry = clipInfo.Entries.Find(ei => Mathf.Abs(normalizedTime - ei.OutNormalizedTime) < 0.01f);
        Debug.AssertFormat(entry != null, "DibbleOut, entry not found. NormalizedTime: {0}", normalizedTime);

        Vector3 targetMatchingAdjustOut = Vector3.zero;
        Vector3 targetMatchingAdjustIn = Vector3.zero;
        if (mTargetMatching != null)
        {
            targetMatchingAdjustOut = mTargetMatching.GetAdjust(curInfo.shortNameHash, entry.OutNormalizedTime);
            targetMatchingAdjustIn = mTargetMatching.GetAdjust(curInfo.shortNameHash, entry.InNormalizedTime);
        }

        float time = entry.InTime - entry.OutTime;
        Vector3 outPos = entry.OutPosition + targetMatchingAdjustOut;
        Vector3 inPos = entry.InPosition + targetMatchingAdjustIn;
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
        mPlaySpeed = curInfo.speed;
    }

    void DribbleIn(string args)
    {
        Debug.Assert(State == DribbleState.Dribbling);
        Hand hand;
        TryParseInInfo(args, out hand);
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
