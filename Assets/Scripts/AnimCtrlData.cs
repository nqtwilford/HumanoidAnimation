using System;
using System.Text;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class AnimCtrlData : IDataResource
{
    [Serializable]
    public class Layer
    {
        public string Name;
        public StateMachine StateMachine;
        public override string ToString()
        {
            return string.Format("{0} StateMachine:{{\n{1}\n}}", Name, StateMachine);
        }
    }

    [Serializable]
    public class StateMachine
    {
        public State[] States;
        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            foreach (State state in States)
                builder.Append("  State: ").Append(state).AppendLine();
            return builder.ToString().TrimEnd('\r', '\n');
        }
    }

    [Serializable]
    public class State
    {
        public string Name;
        public int NameHash;
        public Motion Motion;
        public override string ToString()
        {
            return string.Format("{0}({1}) Motion: {2}", Name, NameHash, Motion);
        }
    }

    [Serializable]
    public class Motion
    {
        public string Name;
        public int NameHash;
    }

    [Serializable]
    public class Clip : Motion
    {
        public override string ToString()
        {
            return string.Format("{0}(Clip)", Name);
        }
    }

    [Serializable]
    public class BlendTree : Motion
    {
        public string ParamName;
        public float MinThreshold;
        public float MaxThreshold;
        public ChildMotion[] Children;
        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendFormat("{0}(BlendTree) Param:{1} Min:{2} Max:{3} {{\n",
                Name, ParamName, MinThreshold, MaxThreshold);
            foreach (ChildMotion child in Children)
                builder.Append("    Child:").Append(child).AppendLine();
            builder.Append("  }");
            return builder.ToString();
        }

        public bool GetBlendInfo(Animator animator, out float t, out ChildMotion motion0, out ChildMotion motion1)
        {
            motion0 = null;
            motion1 = null;
            t = 0;

            float paramValue = animator.GetFloat(ParamName);
            paramValue = Mathf.Clamp(paramValue, MinThreshold, MaxThreshold);
            for (int i = 0; i < Children.Length - 1; ++i)
            {
                if (Children[i].Threshold <= paramValue && paramValue <= Children[i + 1].Threshold)
                {
                    motion0 = Children[i];
                    motion1 = Children[i + 1];
                    t = (paramValue - motion0.Threshold) / (motion1.Threshold - motion0.Threshold);
                    break;
                }
            }
            return motion0 != null && motion1 != null;
        }
    }

    [Serializable]
    public class ChildMotion
    {
        public Motion Motion;
        public float Threshold;
        public override string ToString()
        {
            return string.Format("Motion:{0} Threshold:{1}", Motion, Threshold);
        }
    }

    public string Name;
    public Layer[] Layers;

    Dictionary<int, BlendTree> mBlendTreeCache = new Dictionary<int, BlendTree>();

    void IDataResource.Initialize()
    {
        CacheBlendTree();
    }

    void CacheBlendTree()
    {
        mBlendTreeCache.Clear();
        foreach (Layer layer in Layers)
        {
            foreach (State state in layer.StateMachine.States)
            {
                if (state.Motion is BlendTree)
                    mBlendTreeCache.Add(state.NameHash, state.Motion as BlendTree);
            }
        }
    }

    public BlendTree GetBlendTree(int nameHash)
    {
        BlendTree blendTree = null;
        mBlendTreeCache.TryGetValue(nameHash, out blendTree);
        return blendTree;
    }

    public override string ToString()
    {
        StringBuilder builder = new StringBuilder();
        builder.AppendLine(Name);
        foreach (Layer layer in Layers)
            builder.Append("Layer:").Append(layer).AppendLine();
        return builder.ToString().TrimEnd('\r', '\n');
    }
}
