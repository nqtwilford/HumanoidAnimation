using System;
using System.Text;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class TargetMatchingData : ScriptableObject
{
    [Serializable]
    public class Entry
    {
        public AvatarTarget TargetBodyPart;
        public float StartNormalizedTime;
        public float TargetNormalizedTime;
        public Vector3[] StartPosition = new Vector3[(int)BodyType.Count];
        public Vector3[] TargetPosition = new Vector3[(int)BodyType.Count];
        public Vector3 PositionWeight;

        public override string ToString()
        {
            return string.Format("{0} Time:{1:F4} -> {2:F4} Weight:{3:F1}", TargetBodyPart,
                StartNormalizedTime, TargetNormalizedTime, PositionWeight);
        }
    }

    [Serializable]
    public class Clip
    {
        public int NameHash;
        public string ClipName;
        public List<Entry> Entries = new List<Entry>();

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            builder.Append(ClipName).Append("(").Append(NameHash).Append(") ");
            foreach (Entry entry in Entries)
                builder.Append(entry).Append(' ');
            return builder.ToString();
        }
    }

    [SerializeField]
    private List<Clip> mClips = new List<Clip>();

    public Clip GetClipData(int nameHash)
    {
        return mClips.Find(c => c.NameHash == nameHash);
    }

    public Clip GetClipData(string clipName)
    {
        return mClips.Find(c => c.ClipName == clipName);
    }

    public void AddClipData(Clip clipData)
    {
        mClips.Add(clipData);
    }

    public void Clear()
    {
        mClips.Clear();
    }

    public override string ToString()
    {
        StringBuilder builder = new StringBuilder();
        builder.AppendLine("Target Matching Data:");
        foreach (Clip clip in mClips)
            builder.AppendLine(clip.ToString());
        return builder.ToString();
    }
}
