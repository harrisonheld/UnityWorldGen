using UnityEngine;
using System;

/// <summary>
/// A perfectly flat heightmap. It returns the same height for all x and z.
/// </summary>
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
