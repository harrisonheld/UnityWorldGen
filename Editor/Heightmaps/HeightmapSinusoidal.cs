using System;
using UnityEngine;

/// <summary>
/// This serves as an example of a heightmap that is a sinusoidal wave. Look at this one if you want to make your own.
/// </summary>
/// 
[CreateAssetMenu(menuName = "WorldGenerator/Sinusoidal Heightmap")]
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
        // h(x,z) = A * sin(sx) * sin(sz)
        return Amplitude * (float)Math.Sin(x / Scale) * (float)Math.Sin(z / Scale);
    }
    public override Vector3 GetNormal(float x, float z)
    {
        // the gradient is equal to the normal
        // so for example, our heightmap function is y = h(x,z) = A * sin(sx) * sin(sz)
        // therefore we have h(x,z) - y = 0. this is our f(x,y,z) = 0
        // f = h(x,z) - y
        // the gradient (and normal) is 
        // Normal = grad(f) = <df/dx, df/dy, df/dz>
        // and we can see from that -y term in f that df/dy = -1
        // this makes our normal point downwards. we don't want this, so we'll flip it to get the other normal
        // Normal = -grad(f) = <-df/dx, -df/dy, -df/dz>
        // Normal = <-df/dx, 1, -df/dz>
        // Normal = <-dh/dx, 1, -dh/dz>
        // Q.E.D.

        //
        // SKIP TO THIS PART IF YOU HATE MATH!!!
        // just find the derivitive of the heightmap function with respect to x and z 
        //

        // Calculate the derivatives with respect to x and with respect to z
        float dx = Amplitude * (float)(1.0 / Scale) * (float)Math.Cos(x / Scale) * (float)Math.Sin(z / Scale);
        float dz = Amplitude * (float)(1.0 / Scale) * (float)Math.Sin(x / Scale) * (float)Math.Cos(z / Scale);

        // Return the normalized normal vector
        return new Vector3(-dx, 1.0f, -dz).normalized;
    }
}
