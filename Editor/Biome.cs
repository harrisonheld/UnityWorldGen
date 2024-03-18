using System;
using System.Collections.Generic;
using UnityEngine;
using WorldGenerator;

[Serializable]
public class Biome
{
    [SerializeField]
    [Tooltip("Select a custom heightmap for the biome.")]
    private HeightmapBase _heightmap;

    [SerializeField]
    [Tooltip("The texture that will be used to paint this biome.")]
    private Texture2D _texture;

    [SerializeField]
    [Tooltip("The skybox that will be used for this biome.")]
    private Material _skybox;

    // _features will store the different features (such as rocks and trees) that are present 
    // within the biome and their frequency and size.
    [SerializeField]
    [Tooltip("The features will appear throughout the biome.")]
    private List<BiomeFeature> _features = new();

    [SerializeField]
    [Range(1, 1000)]
    [Tooltip("How often this biome will appear.")]
    private int _frequencyWeight = 100;

    public HeightmapBase GetHeightmap()
    {
        if (_heightmap == null)
        {
            Debug.LogError("The heightmap for this biome is null. Please assign one.");
        }

        return _heightmap;
    }
    public Texture2D GetTexture()
    {
        if (_texture == null)
        {
            Debug.LogError("The texture for this biome is null. Please assign one.");
        }

        return _texture;
    }

    public List<BiomeFeature> GetFeatures()
    {
        return _features;
    }
    public Material GetSkybox()
    {
        if (_skybox == null)
        {
            Debug.LogError("The skybox for this biome is null. Please assign one.");
        }

        return _skybox;
    }
    public void SetFrequencyWeight(int frequencyWeight)
    {
        _frequencyWeight = frequencyWeight;
    }
    public int GetFrequencyWeight()
    {
        return _frequencyWeight;
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
        // sort features so that less frequent features get a chance to show up first when 
        // adding features (CustomTerrain::GenerateChunk)
        features.Sort((x, y) => x.Frequency.CompareTo(y.Frequency));
        _features = features;
    }
    public void SetSkybox(Material skybox)
    {
        _skybox = skybox;
    }
}