using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Biome
{
    [SerializeField]
    [Tooltip("Select a custom heightmap for the biome.")]
    private HeightmapBase _heightmap;

    [SerializeField]
    [Tooltip("The texture that will be used to paint this biome.")]
    private Material _material;

    public HeightmapBase GetHeightmap()
    {
        return _heightmap;
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