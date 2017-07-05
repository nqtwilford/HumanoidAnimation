﻿using UnityEngine;
using System.Collections.Generic;
using System;

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
        public Vector3 OutPosition;
        public float InTime;
        public float InNormalizedTime;
        public Hand InHand;
        public Vector3 InPosition;
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
