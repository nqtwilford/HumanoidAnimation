using System;
using System.Text;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class MovingData : ScriptableObject
{
    [Serializable]
    public class Clip
    {
        public string ClipName;
        public int NameHash;
        public float[] MovingSpeed = new float[(int)BodyType.Count];

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            builder.Append("ClipName:").Append(ClipName).Append("(").Append(NameHash).Append(")");
            for (int i = 0; i < MovingSpeed.Length; ++i)
                builder.Append(' ').Append(MovingSpeed[i]);
            return builder.ToString();
        }
    }
    public List<Clip> Clips = new List<Clip>();

    public override string ToString()
    {
        StringBuilder builder = new StringBuilder();
        foreach (Clip clip in Clips)
            builder.AppendLine(clip.ToString());
        return builder.ToString();
    }
}
