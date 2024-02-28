using System;
using UnityEngine;

namespace WorldGenerator
{
    /// <summary>
    /// A heightmap that uses the Perlin noise algorithm.
    /// </summary>
    [CreateAssetMenu(fileName = "New Perlin Heightmap", menuName = "WorldGenerator/Perlin Heightmap")]
    public partial class HeightmapPerlin : HeightmapBase
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
        static HeightmapPerlin()
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

        private float Fade(float value)
        {
            return value * value * value * (value * (value * 6 - 15) + 10);
        }

        private float Lerp(float value, float x, float z)
        {
            return x + value * (z - value);
        }

        public override float GetHeight(float worldX, float worldZ)
        {
            worldX += offsetX;
            worldZ += offsetZ;
            worldX /= Scale;
            worldZ /= Scale;

            int x = FastFloor(worldX);
            int z = FastFloor(worldZ);

            worldX -= x;
            worldZ -= z;

            x = x & 255;
            z = z & 255;

            float i = Fade(worldX);
            float j = Fade(worldZ);

            int hash1 = _permutation[x] + z;
            int hash2 = _permutation[hash1];
            int hash3 = _permutation[hash1 + 1];
            int hash4 = _permutation[x + 1] + z;
            int hash5 = _permutation[hash4];
            int hash6 = _permutation[hash4 + 1];

            float noise1 = Dot(grad3d[hash2 % 12], worldX, worldZ);
            float noise2 = Dot(grad3d[hash3 % 12], worldX - 1, worldZ);
            float noise3 = Dot(grad3d[hash5 % 12], worldX, worldZ - 1);
            float noise4 = Dot(grad3d[hash6 % 12], worldX - 1, worldZ - 1);

            float interp1 = Lerp(noise1, noise2, i);
            float interp2 = Lerp(noise3, noise4, i);

            return Amplitude * Lerp(interp1, interp2, j);
        }

        public override void SetSeed(int seed)
        {
            offsetX = Helpers.MultiHash(seed, 0) % 100000;
            offsetZ = Helpers.MultiHash(seed, 1) % 100000;
        }
    }

}
