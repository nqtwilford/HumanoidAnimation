using UnityEngine;

public class StateBehaviour : StateMachineBehaviour
{
    public delegate void Callback(Animator animator, AnimatorStateInfo stateInfo, int layerIndex);
    public event Callback StateEnter;

    MovingData mData;
    int mHashMove;

    void Awake()
    {
        mData = Resources.Load<MovingData>("Data/MovingSpeed");
        //Debug.LogFormat("Moving data:\n{0}", mData);
        mHashMove = Animator.StringToHash("move");
    }

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (stateInfo.shortNameHash != mHashMove)
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
        if (stateInfo.shortNameHash != mHashMove)
            animator.GetComponent<MoveControl>().InMovement = false;
        else
            animator.GetComponent<MoveControl>().InMovement = true;

        if (StateEnter != null)
            StateEnter(animator, stateInfo, layerIndex);
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
        string dataPath = string.Format("Data/{0}.controller", animator.runtimeAnimatorController.name);
        AnimCtrlData data = DataResources.Load<AnimCtrlData>(dataPath);
        var blendTree = data.GetBlendTree(nameHash);
        if (blendTree != null)
        {
            var maxChildMotion = blendTree.Children[blendTree.Children.Length - 1];
            var maxClipData = mData.Clips.Find(c => c.NameHash == maxChildMotion.Motion.NameHash);
            float maxSpeed = maxClipData.MovingSpeed[(int)bodyType];
            float curSpeed = animator.GetComponent<MoveControl>().Speed;
            float strideScale, strideFreq;
            if (curSpeed >= maxSpeed)
            {
                strideScale = 1f;
                strideFreq = curSpeed / maxSpeed;
            }
            else
            {
                strideFreq = 1f;
                strideScale = curSpeed / maxSpeed;
            }
            animator.SetFloat("stride_freq", strideFreq);
            animator.SetFloat("stride_scale", strideScale);
        }
        else
        {
            Debug.Assert(false);
            /*
            var clipData = mData.Clips.Find(c => c.NameHash == nameHash);
            if (clipData != null)
            {
                actionSpeed = clipData.MovingSpeed[(int)bodyType];
                actionSpeedStd = clipData.MovingSpeed[(int)BodyType.Char1_Thin];
            }
            */
        }
    }
}
