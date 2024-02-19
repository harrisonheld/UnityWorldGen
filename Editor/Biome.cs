﻿using System;
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
public class Biome
{
    [SerializeField]
    [Tooltip("Select a custom heightmap for the biome.")]
    private HeightmapBase _heightmap;

    [SerializeField]
    [Tooltip("The texture that will be used to paint this biome.")]
    private Texture2D _texture;

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

    public void SetHeightMap(HeightmapBase heightMap)
    {
        _heightmap = heightMap;
    }

    public void SetTexture(Texture2D texture)
    {
        _texture = texture;
    }
}