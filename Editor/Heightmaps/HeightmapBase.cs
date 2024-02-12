using System;
using UnityEngine;

/// <summary>
/// This is the base class from which all Heightmaps derive.
/// These are idealized surfaces. This class is not concerned about resolution, or tris/quads, or anything like that.
/// </summary>
[Serializable]
public abstract class HeightmapBase : ScriptableObject
{
    public abstract float GetHeight(float x, float z);
}