﻿using System;
using UnityEngine;

/// <summary>
/// A heightmap whose surface function is of the form: y = A * sin(fx) * sin(fz)
/// </summary>

// This attribute will make the scriptable object appear in the Create menu.
// fileName is the default name of the asset when it is created.
// menuName is the path in the Create menu.
[CreateAssetMenu(fileName = "New Sinusoidal Heightmap", menuName = "WorldGenerator/Sinusoidal Heightmap")]
public partial class HeightmapSinusoidal : HeightmapBase
{
    /*
     * SerializeField will make a field appear in the inspector.
     * These are NOT fields, they are properties, which cannot be directly marked with [SerializeField].
     * [field: SerializeField] applies the [SerializeField] attribute to the backing field of the property.
     */
    [field: SerializeField]
    [Tooltip("The amplitude of the wave. This will make peaks and valleys more extreme.")]
    public float Amplitude { get; set; } = 10.0f;
    [field: SerializeField]
    [Tooltip("The scale of the wave. This will stretch the surface in the horizontal directions.")]
    public float Scale { get; set; } = 10.0f;

    public override float GetHeight(float worldX, float worldZ)
    {
        return Amplitude * (float)Math.Sin(worldX / Scale) * (float)Math.Sin(worldZ / Scale);
    }

    public override void SetSeed(int seed)
    {
        // This heightmap does not use a seed.
        // For heightmaps that do, this is where you would set the seed
        // if you want to use the seed, you can take a hash in the GetHeight() method to use it
        // such as Helpers.MultiHash(this._seed, x, y)
        // we are avoiding System.Random() because constructing those is super slow
    }
}
