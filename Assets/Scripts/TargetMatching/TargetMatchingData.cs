using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class TargetMatchingData : ScriptableObject
{
    [Serializable]
    public class Entry
    {
        public AvatarTarget TargetBodyPart;
        public float StartTime;
        public float StartNormalizedTime;
        public float TargetTime;
        public float TargetNormalizedTime;
        public Vector3 StartPosition;
        public Vector3 TargetPosition;
        public Vector3 PositionWeight;
    }

    [Serializable]
    public class Clip
    {
        public int NameHash;
        public string ClipName;
        public List<Entry> Entries = new List<Entry>();
    }

    public List<Clip> Clips = new List<Clip>();
}
