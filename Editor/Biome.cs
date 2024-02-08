using System;
using UnityEngine;

[Serializable]
public class Biome
{
    [SerializeField]
    [Tooltip("Select a custom heightmap for the biome.")]
    private HeightmapBase _heightmap;

    [SerializeField]
    [Tooltip("The higher the frequency of this biome, the more often it will occur relative to other biomes.")]
    private float frequency = 10;

    public HeightmapBase GetHeightmap()
    {
        return _heightmap;
    }
    public float GetFrequency()
    {
        return frequency;
    }
}