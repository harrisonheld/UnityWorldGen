using System;
using UnityEngine;

[Serializable]
public class Biome
{
    public string name;
    public string description;

    [SerializeField]
    [Tooltip("Select a custom heightmap for the biome.")]
    private Heightmap _heightmap;

    [SerializeField]
    [Tooltip("The higher the frequency of this biome, the more often it will occur relative to other biomes.")]
    private float frequency = 10;

    [SerializeField]
    [Tooltip("The texture that will be used to paint this biome.")]
    private Material _material;

    public Heightmap GetHeightmap()
    {
        return _heightmap;
    }
    public float GetFrequency()
    {
        return frequency;
    }
    public Material GetMaterial()
    {
        if (_material == null)
        {
            Debug.LogError("The material for this biome is null. Please assign a texture to the biome.");
        }

        return _material;
    }
}