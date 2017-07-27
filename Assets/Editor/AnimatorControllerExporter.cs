using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;

public static class AnimatorControllerExporter
{
    [MenuItem("Animation/Export State Machine")]
    public static void Export()
    {
        var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>("Assets/Resources/Controllers/All.controller");
        AnimCtrlData data = null;
        if (Export(controller, out data))
        {
            DataResources.Save(Application.dataPath + "/Resources/Data/All.controller.bytes", data);
            Debug.LogFormat("动画状态机数据导出成功！！！\n{0}", data);
            AssetDatabase.Refresh();
        }
        else
            Debug.LogError("动画状态机数据导出失败！！！");
    }

    public static bool Export(AnimatorController controller, out AnimCtrlData data)
    {
        data = new AnimCtrlData()
        {
            Name = controller.name,
            Layers = new AnimCtrlData.Layer[controller.layers.Length]
        };
        // Layers
        for (int i = 0; i < controller.layers.Length; ++i)
        {
            var layer = controller.layers[i];
            var layerData = data.Layers[i] = new AnimCtrlData.Layer()
            {
                Name = layer.name,
                StateMachine = new AnimCtrlData.StateMachine()
                {
                    States = new AnimCtrlData.State[layer.stateMachine.states.Length],
                },
            };
            // States
            for (int j = 0; j < layer.stateMachine.states.Length; ++j)
            {
                var state = layer.stateMachine.states[j];
                if (state.state.motion.name != state.state.name)
                {
                    Debug.LogErrorFormat("状态名和动作名不一致。状态：{0}", state.state.name);
                    return false;
                }
                var stateData = layerData.StateMachine.States[j] = new AnimCtrlData.State()
                {
                    Name = state.state.name,
                    NameHash = state.state.nameHash,
                };
                // Motion
                Motion motion = state.state.motion;
                if (motion is BlendTree)    // BlendTree
                {
                    BlendTree blendTree = motion as BlendTree;
                    var blendTreeData = new AnimCtrlData.BlendTree()
                    {
                        Name = motion.name,
                        NameHash = Animator.StringToHash(motion.name),
                        ParamName = blendTree.blendParameter,
                        MinThreshold = blendTree.minThreshold,
                        MaxThreshold = blendTree.maxThreshold,
                        Children = new AnimCtrlData.ChildMotion[blendTree.children.Length],
                    };
                    stateData.Motion = blendTreeData;
                    for (int k = 0; k < blendTree.children.Length; ++k)
                    {
                        var childMotion = blendTree.children[k];
                        if (childMotion.motion is BlendTree)
                        {
                            Debug.LogErrorFormat("暂不支持嵌套混合树！ {0}/{1}/{2}/{3}/{4}",
                                controller.name, layer.name, state.state.name, motion.name, childMotion.motion.name);
                            return false;
                        }
                        blendTreeData.Children[k] = new AnimCtrlData.ChildMotion()
                        {
                            Motion = new AnimCtrlData.Clip()
                            {
                                Name = childMotion.motion.name,
                                NameHash = Animator.StringToHash(childMotion.motion.name),
                            },
                            Threshold = childMotion.threshold,
                        };
                        blendTreeData.Children[k].Motion.Name = childMotion.motion.name;
                    }
                }
                else        // Clip
                {
                    stateData.Motion = new AnimCtrlData.Clip()
                    {
                        Name = motion.name,
                        NameHash = Animator.StringToHash(motion.name),
                    };
                }
            }
        }
        return true;
    }
}