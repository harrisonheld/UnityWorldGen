using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Biome is a data class that holds all the information needed to generate a biome.
/// A heightmap to generate the surface.
/// A texture to paint the surface.
/// A list of features to be generated in this biome.
/// A skybox to be rendered when the camera is in this biome.
/// </summary>
[Serializable]
public class Biome {
    [SerializeField]
    private string _biomeId;

    [SerializeField]
    [Tooltip("Name of the current biome.")]
    private string _name;

    [SerializeField]
    [Tooltip("Select a custom heightmap for the biome.")]
    private HeightmapBase _heightmap;

    [SerializeField]
    [Tooltip("The texture that will be used to paint this biome.")]
    private Texture2D _texture;

    public string GetBiomeId()
    {
        return _biomeId;
    }

    public string GetName()
    {
        return _name;
    }

    [SerializeField]
    [Range(1, 1000)]
    [Tooltip("How often this biome will appear.")]
    private int _frequencyWeight = 100;

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

    public void SetBiomeId(string biomeId)
    {
        _biomeId = biomeId;
    }

    public void SetName(string name)
    {
        _name = name;
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
}
