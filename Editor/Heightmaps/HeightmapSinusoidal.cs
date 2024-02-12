using System;
using UnityEngine;

/// <summary>
/// This serves as an example of a heightmap that is a sinusoidal wave. Look at this one if you want to make your own.
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

    public override float GetHeight(float x, float z)
    {
        return Amplitude * (float)Math.Sin(x / Scale) * (float)Math.Sin(z / Scale);
    }
}
