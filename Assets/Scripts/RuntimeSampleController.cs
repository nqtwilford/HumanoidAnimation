using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public interface IRuntimeSampler
{
    void OnStartClip(AnimationClip clip);
    void OnFinishClip(AnimationClip clip);
    void OnComplete();
}

[RequireComponent(typeof(Animator))]
public class RuntimeSampleController : MonoBehaviour
{
    public bool IsCompleted { get; private set; }
    public ICollection<IRuntimeSampler> Samplers = new List<IRuntimeSampler>();

    Animator mAnimator;
    IEnumerator mIterClips;
    bool mStarted;

    void Awake()
    {
        mAnimator = GetComponent<Animator>();
        RuntimeAnimatorController controller = mAnimator.runtimeAnimatorController;
        Debug.Assert(controller != null);
        mIterClips = controller.animationClips.GetEnumerator();
    }

    IEnumerator Start()
    {
        mAnimator.enabled = true;
        yield return new WaitForSeconds(0.5f);
        StartNextClip();
        mStarted = true;
    }

    void Update()
    {
        if (mStarted && !IsCompleted)
        {
            var curStateInfo = mAnimator.GetCurrentAnimatorStateInfo(0);
            //Debug.LogFormat("{0} NormalizedTime:{1}", name, curStateInfo.normalizedTime);
            if (curStateInfo.normalizedTime >= 1)
            {
                FinishCurClip();
                StartNextClip();
            }
        }
    }

    void FinishCurClip()
    {
        var curClip = mIterClips.Current as AnimationClip;
        foreach (var sampler in Samplers)
            sampler.OnFinishClip(curClip);
    }

    void StartNextClip()
    {
        if (mIterClips.MoveNext())
        {
            transform.position = Vector3.zero;
            transform.forward = Vector3.forward;
            var clip = mIterClips.Current as AnimationClip;
            mAnimator.Play(clip.name, 0, 0);
            foreach (var sampler in Samplers)
                sampler.OnStartClip(clip);
        }
        else
        {
            IsCompleted = true;
            foreach (var sampler in Samplers)
                sampler.OnComplete();
        }
    }
}
