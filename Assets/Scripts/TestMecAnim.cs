using UnityEngine;

[RequireComponent(typeof(Animator), typeof(DribbleSimulator))]
public class TestMecAnim : MonoBehaviour
{
    Animator mAnimator;
    DribbleSimulator mDribbleSim;
    Vector3 mInitialPos;

    void Awake()
    {
        mInitialPos = transform.position;
        transform.Find("BallGeo").gameObject.SetActive(false);
        transform.Find("BallGeoL").gameObject.SetActive(false);
        transform.Find("BallGeoR").gameObject.SetActive(false);
        mAnimator = GetComponent<Animator>();
        mDribbleSim = GetComponent<DribbleSimulator>();
    }

    void Start()
    {
        mDribbleSim.State = DribbleState.InHand;
        mDribbleSim.Hand = Hand.Right;
    }

    void Update()
    {
        if (Input.GetKeyUp(KeyCode.R))
        {
            transform.position = mInitialPos;
            mAnimator.Play("None", 0, 0);
        }
        if (Input.GetKeyUp(KeyCode.Space))
        {
            mDribbleSim.State = DribbleState.InHand;
            if (!mDribbleSim.mMirror)
            {
                mDribbleSim.Hand = Hand.Right;
                mAnimator.SetBool("mirror", false);
            }
            else
            {
                mDribbleSim.Hand = Hand.Left;
                mAnimator.SetBool("mirror", true);
            }
            //mAnimator.CrossFade("spinmove", 0.2f, 0, -0.2f);
            mAnimator.Play("spinmove", 0);
            //mAnimator.SetTrigger("cross_over");
        }
        var transInfo = mAnimator.GetAnimatorTransitionInfo(0);
        if (transInfo.normalizedTime > 0 && transInfo.normalizedTime <= 1)
        {
            Debug.LogFormat("Transition normalized time:{0}", transInfo.normalizedTime);
        }
    }
}
