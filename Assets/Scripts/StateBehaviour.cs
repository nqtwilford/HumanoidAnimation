using UnityEngine;

public class StateBehaviour : StateMachineBehaviour
{
    MovingData mData;
    int mHashRun;
    int mHashStand;

    void Awake()
    {
        mData = Resources.Load<MovingData>("Data/MovingSpeed");
        //Debug.LogFormat("Moving data:\n{0}", mData);
        mHashRun = Animator.StringToHash("run");
        mHashStand = Animator.StringToHash("standwithball");
    }

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        ModifySpeedMultiplier(animator, stateInfo.shortNameHash);
        //var curStateInfo = animator.GetCurrentAnimatorStateInfo(layerIndex);
        //if (curStateInfo.shortNameHash == mHashRun)
        //    Debug.LogFormat("Transition from run, nor time:{0}", curStateInfo.normalizedTime);
        //if (stateInfo.shortNameHash == mHashStand)
        //    Debug.LogFormat("Enter stand, nor time:{0}", stateInfo.normalizedTime);
        //if (curStateInfo.shortNameHash == mHashRun && stateInfo.shortNameHash == mHashStand)
        //{
        //    animator.ForceStateNormalizedTime(curStateInfo.normalizedTime);
        //    //Debug.Break();
        //}
        if (stateInfo.shortNameHash != mHashRun)
            animator.GetComponent<MoveControl>().InMovement = false;
        else
            animator.GetComponent<MoveControl>().InMovement = true;
    }

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        //if (stateInfo.shortNameHash == mHashRun)
        //    Debug.LogFormat("Exit run, nor time:{0}", stateInfo.normalizedTime);
    }

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        ModifySpeedMultiplier(animator, stateInfo.shortNameHash);
    }

    void ModifySpeedMultiplier(Animator animator, int nameHash)
    {
        BodyType bodyType = animator.GetComponent<DribbleSimulator>().BodyType;
        var clipData = mData.Clips.Find(c => c.NameHash == nameHash);
        if (clipData != null)
        {
            float actionSpeed = clipData.MovingSpeed[(int)bodyType];
            float moveSpeed = animator.GetComponent<MoveControl>().Speed;
            if (moveSpeed >= 3f)
            {
                float speedMultiplier = moveSpeed / actionSpeed;
                animator.SetFloat("speed_multiplier", speedMultiplier);
            }
            else
                animator.SetFloat("speed_multiplier", 1f);
        }
    }
}
