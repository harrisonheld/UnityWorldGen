using System;
using UnityEngine;

/// <summary>
/// A heightmap that uses the Simplex noise algorithm.
/// </summary>
[CreateAssetMenu(fileName = "New Simplex Heightmap", menuName = "WorldGenerator/Simplex Heightmap")]
public partial class HeightmapSimplex : HeightmapBase
{
    [field: SerializeField]
    [Tooltip("The amplitude of the noise. This will make peaks and valleys more extreme.")]
    public float Amplitude { get; set; } = 100.0f;

    [field: SerializeField]
    [Tooltip("The scale of the noise. This will make the noise more or less frequent.")]
    public float Scale { get; set; } = 10.0f;

    private float offsetX = 0;
    private float offsetZ = 0;
    private static readonly int[] _permutation = new int[512];
    static HeightmapSimplex()
    {
        int[] _permTemp = {
            241, 126, 133, 49, 234, 73, 255, 200, 112, 99, 249, 217, 135, 219, 31, 89,
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
            71, 204, 52, 143, 96, 160, 39, 85, 123, 66, 122, 203, 20, 131, 13, 182, 151
        };

        for (int i = 0; i < 512; i++)
        {
            _permutation[i] = _permTemp[i & 255];
        }
    }
    private int FastFloor(float value)
    {
        return value >= 0 ? (int)value : (int)value - 1;
    }
    private float Dot(int[] g, float x, float y)
    {
        return g[0] * x + g[1] * y;
    }

    private int[][] grad3d = {
        new int[]{1,1,0}, new int[]{-1,1,0}, new int[]{1,-1,0}, new int[]{-1,-1,0},
        new int[]{1,0,1}, new int[]{-1,0,1}, new int[]{1,0,-1}, new int[]{-1,0,-1},
        new int[]{0,1,1}, new int[]{0,-1,1}, new int[]{0,1,-1}, new int[]{0,-1,-1}
    };

    public override float GetHeight(float worldX, float worldZ)
    {
        worldX += offsetX;
        worldZ += offsetZ;
        worldX /= Scale;
        worldZ /= Scale;

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
        int hash1 = _permutation[temp_i + _permutation[temp_j]] % 12;
        int hash2 = _permutation[temp_i + i1 + _permutation[temp_j + j1]] % 12;
        int hash3 = _permutation[temp_i + 1 + _permutation[temp_j + 1]] % 12;

        float t1 = 0.5f - x1 * x1 - z1 * z1;
        if (t1 < 0.0f)
        {
            noise1 = 0.0f;
        }
        else
        {
            t1 *= t1;
            noise1 = t1 * t1 * Dot(grad3d[hash1], x1, z1);
        }

        float t2 = 0.5f - x2 * x2 - z2 * z2;
        if (t2 < 0.0f)
        {
            noise2 = 0.0f;
        }
        else
        {
            t2 *= t2;
            noise2 = t2 * t2 * Dot(grad3d[hash2], x2, z2);
        }

        float t3 = 0.5f - x3 * x3 - z3 * z3;
        if (t3 < 0.0f)
        {
            noise3 = 0.0f;
        }
        else
        {
            t3 *= t3;
            noise3 = t3 * t3 * Dot(grad3d[hash3], x3, z3);
        }

        return Amplitude * (noise1 + noise2 + noise3);
    }

    public override void SetSeed(int seed)
    {
        offsetX = Helpers.MultiHash(seed, 0) % 100000;
        offsetZ = Helpers.MultiHash(seed, 1) % 100000;
    }
}