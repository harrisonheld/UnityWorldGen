using System;
using UnityEngine;

[CreateAssetMenu(fileName = "New Simplex Heightmap", menuName = "WorldGenerator/Simplex Heightmap")]
public partial class HeightmapSimplex : HeightmapBase
{
    [SerializeField]
    int howdy;

    [field: SerializeField]
    [Tooltip("blah.")]
    public float Parameter { get; set; } = 10.0f;
    public override float GetHeight(float worldX, float worldZ)
    {
        // TODO: Implement Simplex noise
        return 0;
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
