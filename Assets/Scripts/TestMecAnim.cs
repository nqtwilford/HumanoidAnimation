using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Animator), typeof(DribbleSimulator))]
public class TestMecAnim : MonoBehaviour
{
    Animator mAnimator;
    DribbleSimulator mDribbleSim;
    //Vector3 mInitialPos;

    void Awake()
    {
        //mInitialPos = transform.position;
        Transform trans = transform.Find("BallGeo");
        if (trans != null)
            trans.gameObject.SetActive(false);
        trans = transform.Find("BallGeoL");
        if (trans != null)
            trans.gameObject.SetActive(false);
        trans = transform.Find("BallGeoR");
        if (trans != null)
            trans.gameObject.SetActive(false);
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
        if (Input.GetKeyUp(KeyCode.J))
        {
            StartCoroutine(Shoot());
        }
        else if (Input.GetKeyUp(KeyCode.U))
        {
            StartCoroutine(Dunk());
        }
        else if (Input.GetKeyUp(KeyCode.I))
        {
            StartCoroutine(CrossOver());
        }
        else if (Input.GetKeyUp(KeyCode.K))
        {
            StartCoroutine(Pass());
        }
        /*
        var transInfo = mAnimator.GetAnimatorTransitionInfo(0);
        if (transInfo.normalizedTime > 0 && transInfo.normalizedTime <= 1)
        {
            Debug.LogFormat("Transition normalized time:{0}", transInfo.normalizedTime);
        }
        */
        if (Input.GetKeyUp(KeyCode.C))
            ClearConsole();
    }

    void ClearConsole()
    {
        var assembly = System.Reflection.Assembly.GetAssembly(typeof(UnityEditor.ActiveEditorTracker));
        var type = assembly.GetType("UnityEditorInternal.LogEntries");
        var method = type.GetMethod("Clear");
        method.Invoke(new object(), null);
    }

    IEnumerator CrossOver()
    {
        while (mDribbleSim.State != DribbleState.InHand)
            yield return null;
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
        //mAnimator.Play("spinmove", 0);
        mAnimator.SetTrigger("cross_over");
    }

    IEnumerator Pass()
    {
        while (mDribbleSim.State != DribbleState.InHand)
            yield return null;
        mAnimator.SetTrigger("pass");
    }

    IEnumerator Shoot()
    {
        while (mDribbleSim.State != DribbleState.InHand)
            yield return null;
        mAnimator.SetTrigger("shoot");
    }

    IEnumerator Dunk()
    {
        while (mDribbleSim.State != DribbleState.InHand)
            yield return null;
        mAnimator.SetTrigger("dunk");
    }

    void OnGUI()
    {
        GUILayout.BeginHorizontal();
        Time.timeScale = GUILayout.HorizontalSlider(Time.timeScale, 0, 1f, GUILayout.Width(500));
        GUILayout.Label(Time.timeScale.ToString("F3"));
        GUILayout.EndHorizontal();
    }
}
