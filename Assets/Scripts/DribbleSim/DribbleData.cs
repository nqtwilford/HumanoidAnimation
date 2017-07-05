using UnityEngine;
using System.Collections.Generic;
using System;

[Serializable]
public class DribbleData : ScriptableObject
{
    [Serializable]
    public class Entry
    {
        public DribbleType Type;
        public float ReleaseTime;
        public float ReleaseNormalizedTime;
        public Hand ReleaseHand;
        public Vector3 ReleasePosition;
        public float RegainTime;
        public float RegainNormalizedTime;
        public Hand RegainHand;
        public Vector3 RegainPosition;
    }

    [Serializable]
    public class Clip
    {
        public int NameHash;
        public string ClipName;
        public List<Entry> Events = new List<Entry>();
    }

    public List<Clip> Infos = new List<Clip>();
}
