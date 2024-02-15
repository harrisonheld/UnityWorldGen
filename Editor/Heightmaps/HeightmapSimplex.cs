using System;
using UnityEngine;

[CreateAssetMenu(fileName = "New Simplex Heightmap", menuName = "WorldGenerator/Simplex Heightmap")]
public partial class HeightmapSimplex : HeightmapBase
{
    /*
     * SerializeField will make a field appear in the inspector.
     * These are NOT fields, they are properties, which cannot be directly marked with [SerializeField].
     * [field: SerializeField] applies the [SerializeField] attribute to the backing field of the property.
     */
    [field: SerializeField]
    [Tooltip("The amplitude of the wave. This will make peaks and valleys more extreme.")]
    public float Amplitude { get; set; } = 2.0f;

    public int FastFloor(float value) 
    {
        return value >= 0 ? (int)value : (int)value - 1;
    }

    public float grad(int hash, float x, float y)
    {
        int h = hash & 0x3F;
        float u = h < 4 ? x : y;
        float v = h < 4 ? y : x;
        return ((h & 1) != 0 ? -u : u) + ((h & 2) != 0 ? -2.0f * v : 2.0f * v);
    }

    public float dot(int[] g, float x, float y) {
        return g[0]*x + g[1]*y;
    }

      private static int[] perm_temp = 
        {241, 126, 133, 49, 234, 73, 255, 200, 112, 99, 249, 217, 135, 219, 31, 89, 
        115, 141, 170, 218, 230, 172, 164, 106, 15, 254, 181, 169, 157, 26, 41, 144, 
        119, 224, 229, 44, 113, 187, 18, 110, 12, 5, 72, 250, 62, 45, 235, 225, 252, 
        70, 60, 174, 69, 248, 27, 88, 56, 28, 128, 76, 153, 121, 0, 194, 107, 177, 147, 
        57, 54, 102, 64, 38, 37, 207, 152, 226, 140, 186, 94, 65, 145, 237, 82, 117, 6, 
        208, 32, 17, 136, 47, 2, 7, 201, 4, 75, 29, 210, 191, 199, 118, 167, 246, 221, 
        184, 114, 139, 155, 173, 55, 231, 222, 59, 196, 91, 178, 148, 183, 158, 216, 
        195, 166, 137, 125, 90, 42, 213, 202, 236, 232, 215, 190, 192, 233, 101, 3, 
        98, 179, 242, 79, 180, 168, 40, 120, 46, 24, 111, 51, 161, 228, 103, 212, 205, 
        159, 116, 77, 67, 10, 86, 209, 165, 214, 63, 16, 220, 104, 150, 22, 227, 34, 
        134 ,154, 129, 1, 14, 253, 244, 81, 240, 33, 124, 146, 61, 43, 53, 35, 239, 80, 
        162, 21, 100, 95, 211, 68, 23, 83, 11, 48, 243, 132, 176, 198, 50, 58, 84, 223, 
        30, 127, 19, 175, 185, 109, 247, 93, 25, 188, 206, 193, 149, 74, 8, 189, 197, 
        142, 163, 36, 108, 171, 245, 97, 130, 251, 105, 138, 78, 9, 92, 87, 238, 156, 
        71, 204, 52, 143, 96, 160, 39, 85, 123, 66, 122, 203, 20, 131, 13, 182, 151};

     private static int[] permutation = new int[512];

    static HeightmapSimplex() 
    {
        for (int i = 0; i < 512; i++) 
        {
            permutation[i] = perm_temp[i & 255];
        }
    }


    public int[][] grad3d = {
        new int[]{1,1,0}, new int[]{-1,1,0}, new int[]{1,-1,0}, new int[]{-1,-1,0},
        new int[]{1,0,1}, new int[]{-1,0,1}, new int[]{1,0,-1}, new int[]{-1,0,-1},
        new int[]{0,1,1}, new int[]{0,-1,1}, new int[]{0,1,-1}, new int[]{0,-1,-1}
    };

    public override float GetHeight(float worldX, float worldZ)
    {
        float noise1, noise2, noise3;

        float SkewFactor = ((float)Math.Sqrt(3) - 1.0f) * 0.5f;
        float UnskewFactor = (3.0f - (float)Math.Sqrt(3)) / 6.0f;

        float skew = (worldX + worldZ) * SkewFactor;
        int i = FastFloor(worldX + skew);
        int j = FastFloor(worldZ + skew);

        float unskew = (i + j) * UnskewFactor;
        float dx = i - unskew;
        float dz = j - unskew;
        float x1 = worldX - dx;
        float z1 = worldZ - dz;

        int i1, j1;
        if (x1 > z1) 
        {
            i1 = 1;
            j1 = 0;
        } 
        else 
        {
            i1 = 0;
            j1 = 1;
        }

        float x2 = x1 - i1 + UnskewFactor;
        float z2 = z1 - j1 + UnskewFactor;
        float x3 = x1 - 1.0f + 2.0f * UnskewFactor;
        float z3 = z1 - 1.0f + 2.0f * UnskewFactor;

        int temp_i = i & 255;
        int temp_j = j & 255;
        int hash1 = permutation[temp_i + permutation[temp_j]] % 12;
        int hash2 = permutation[temp_i + i1 + permutation[temp_j + j1]] % 12;
        int hash3 = permutation[temp_i + 1 + permutation[temp_j + 1]] % 12;

        float t1 = 0.5f - x1 * x1 - z1 * z1;
        if (t1 < 0.0f)
        {
            noise1 = 0.0f;
        }
        else 
        {
            t1 *= t1;
            noise1 = t1 * t1 * dot(grad3d[hash1], x1, z1);
        }

        float t2 = 0.5f - x2 * x2 - z2 * z2;
        if (t2 < 0.0f)
        {
            noise2 = 0.0f;
        }
        else 
        {
            t2 *= t2;
            noise2 = t2 * t2 * dot(grad3d[hash2], x2, z2);
        }

        float t3 = 0.5f - x3 * x3 - z3 * z3;
        if (t3 < 0.0f)
        {
            noise3 = 0.0f;
        }
        else 
        {
            t3 *= t3;
            noise3 = t3 * t3 * dot(grad3d[hash3], x3, z3);
        }

        return Amplitude * (noise1 + noise2 + noise3);
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


