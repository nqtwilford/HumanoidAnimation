using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class TestOverrideController : MonoBehaviour
{
    [System.Serializable]
    struct OverridePair
    {
        public string Name;
        public AnimationClip Clip;
    }
    [SerializeField]
    OverridePair[] Overrides;

    Animator mAnimator;

    void Awake()
    {
        mAnimator = GetComponent<Animator>();
    }

    void Start()
    {
        AnimatorOverrideController overrideController = new AnimatorOverrideController();
        foreach (var pair in Overrides)
        {
            overrideController[pair.Name] = pair.Clip;
        }
        overrideController.runtimeAnimatorController = mAnimator.runtimeAnimatorController;
        mAnimator.runtimeAnimatorController = overrideController;
    }
}
