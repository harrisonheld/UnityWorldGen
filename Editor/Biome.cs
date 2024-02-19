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
    private Texture2D _texture;

    // _features will store the different features (such as rocks and trees) that are present 
    // within the biome and their frequency and size.
    [SerializeField]
    [Tooltip("The features will appear throughout the biome.")]
    private List<BiomeFeature> _features;

    public HeightmapBase GetHeightmap()
    {
        return _heightmap;
    }
    public Texture2D GetTexture()
    {
        if (_texture == null)
        {
            Debug.LogError("The texture for this biome is null. Please assign a texture to the biome.");
        }

        return _texture;
    }

    public List<BiomeFeature> GetFeatures()
    {
        return _features;
    }

    public void SetHeightMap(HeightmapBase heightMap)
    {
        _heightmap = heightMap;
    }

    public void SetTexture(Texture2D texture)
    {
        _texture = texture;
    }

    public void SetFeatures(List<BiomeFeature> features)
    {
        _features = features;
    }
}