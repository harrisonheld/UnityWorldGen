using System;
using System.Diagnostics;
using UnityEngine;

[Serializable]
public class Heightmap
{
    [SerializeField]
    [Tooltip("Select a custom heightmap for the biome.")]
    private HeightmapBase _type;

    [Range(0f, 100f)]
    [SerializeField]
    private float _scale;

    [SerializeField]
    private int _octaves;

    public HeightmapBase GetHeightmap()
    {
        return _type;
    }

    public float GetScale()
    {
        return _scale = 10;
    }

    public float GetOctaves()
    {
        return _octaves = 4;
    }
}