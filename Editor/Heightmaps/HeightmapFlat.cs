using UnityEngine;
using System;

public partial class HeightmapFlat : HeightmapBase
{
    [field: SerializeField]
    public float Height { get; set; }
    public override float GetHeight(float x, float z)
    {
        return Height;
    }
    public override Vector3 GetNormal(float x, float z)
    {
        return Vector3.up;
    }
}
