using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour
{
    public bool Mirror;

    Animator mAnimator;

    // Use this for initialization
    void Start()
    {
        mAnimator = GetComponent<Animator>();
        mAnimator.SetBool("mirror", Mirror);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyUp(KeyCode.Space))
        {
            if (Mirror)
                mAnimator.Play("mirror_run");
            else
                mAnimator.Play("run");
            Debug.Break();
        }
    }

    void DribbleOut()
    {
    }

    void GroundBounce()
    {
    }

    void DribbleIn()
    {
    }
}
