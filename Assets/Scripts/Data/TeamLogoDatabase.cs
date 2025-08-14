using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "GridironGM/Team Logo Database", fileName = "TeamLogoDatabase")]
public class TeamLogoDatabase : ScriptableObject
{
    [Serializable]
    public struct Entry { public string abbreviation; public Sprite sprite; }
    public List<Entry> entries = new();
}

