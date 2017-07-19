using System;
using System.Text;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class DribbleData : ScriptableObject
{
    [Serializable]
    public class Entry
    {
        public DribbleType Type;
        public float OutTime;
        public float OutNormalizedTime;
        public Hand OutHand;
        public Vector3[] OutPosition = new Vector3[(int)BodyType.Count];
        public float InTime;
        public float InNormalizedTime;
        public Hand InHand;
        public Vector3[] InPosition = new Vector3[(int)BodyType.Count];

        public override string ToString()
        {
            return string.Format("Dribble type:{0} ({1:F4}|{2:F4}|{3}|{4}) -> ({5:F4}|{6:F4}|{7}|{8})",
                Type, OutTime, OutNormalizedTime, OutHand, OutPosition[(int)BodyType.Char1_Thin].ToString("F3"),
                InTime, InNormalizedTime, InHand, InPosition[(int)BodyType.Char1_Thin].ToString("F3"));
        }
    }

    [Serializable]
    public class Clip
    {
        public int NameHash;
        public string ClipName;

        [SerializeField]
        private List<Entry> Entries = new List<Entry>();

        public Entry GetEntry(float outNormalizedTime, float deviation = float.MaxValue)
        {
            return Entries.Find(e => e.OutNormalizedTime <= outNormalizedTime && 
                outNormalizedTime <= e.InNormalizedTime && 
                Mathf.Abs(outNormalizedTime - e.OutNormalizedTime) <= deviation);
        }

        public void AddEntry(Entry entry)
        {
            Entries.Add(entry);
        }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            builder.Append(ClipName).Append("(").Append(NameHash).AppendLine(") ");
            foreach (var entry in Entries)
                builder.AppendLine(entry.ToString());
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
        builder.AppendLine("Dribble data:");
        foreach (Clip clip in mClips)
            builder.AppendLine(clip.ToString());
        return builder.ToString();
    }
}
