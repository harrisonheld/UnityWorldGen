using UnityEngine;
using System;

[CreateAssetMenu(fileName = "New Flat Heightmap", menuName = "WorldGenerator/Flat Heightmap")]
public partial class HeightmapFlat : HeightmapBase
{
    [field: SerializeField]
    [Tooltip("The y-level of the flat surface.")]
    public float Height { get; set; }
    public override float GetHeight(float x, float z)
    {
        return Height;
    }
}
