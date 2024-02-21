using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Biome
{
    // [SerializeField]
    //
    // private string _name = "Test Nameeeee";

    [SerializeField]
    [Tooltip("Select a custom heightmap for the biome.")]
    private HeightmapBase _heightmap;

    [SerializeField]
    [Tooltip("The texture that will be used to paint this biome.")]
    private Texture2D _texture;

    // public string GetName()
    // {
    //     return name;
    // }

    // Property to access _name
    // public string Name
    // {
    //     get { return _name; }
    //     set { _name = value; }
    // }

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

    // public void SetName(string name)
    // {
    //     name = name;
    // }

    public void SetHeightMap(HeightmapBase heightMap)
    {
        _heightmap = heightMap;
    }

    public void SetTexture(Texture2D texture)
    {
        _texture = texture;
    }
}
