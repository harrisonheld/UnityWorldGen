using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Simplex Heightmap", menuName = "WorldGenerator/Simplex Heightmap")]
public class HeightmapSimplex : MonoBehaviour
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

    public int FastFloor(float value) 
    {
        return value >= 0 ? (int)value : (int)value - 1;
    }

    int[] permutation = [241 126 133  49 234  73 255 200 112  99 249 217 135 219  31  89 115 141
 170 218 230 172 164 106  15 254 181 169 157  26  41 144 119 224 229  44
 113 187  18 110  12   5  72 250  62  45 235 225 252  70  60 174  69 248
  27  88  56  28 128  76 153 121   0 194 107 177 147  57  54 102  64  38
  37 207 152 226 140 186  94  65 145 237  82 117   6 208  32  17 136  47
   2   7 201   4  75  29 210 191 199 118 167 246 221 184 114 139 155 173
  55 231 222  59 196  91 178 148 183 158 216 195 166 137 125  90  42 213
 202 236 232 215 190 192 233 101   3  98 179 242  79 180 168  40 120  46
  24 111  51 161 228 103 212 205 159 116  77  67  10  86 209 165 214  63
  16 220 104 150  22 227  34 134 154 129   1  14 253 244  81 240  33 124
 146  61  43  53  35 239  80 162  21 100  95 211  68  23  83  11  48 243
 132 176 198  50  58  84 223  30 127  19 175 185 109 247  93  25 188 206
 193 149  74   8 189 197 142 163  36 108 171 245  97 130 251 105 138  78
   9  92  87 238 156  71 204  52 143  96 160  39  85 123  66 122 203  20
 131  13 182 151]

    public override float GetHeight(float worldX, float worldZ)
    {
        float SkewFactor = ((float)Math.sqrt(3) - 1) / 2;
        float UnskewFactor = (float)(SkewFactor / 6);

        float skew = (worldX + worldZ) * SkewFactor;
        int i = FastFloor(worldX + skew);
        int j = FastFloor(worldZ + skew);

        float unskew = (i + j) * UnskewFactor;
        float x = i - unskew;
        float z = j - unskew;
        float dx = worldX - x;
        float dz = worldZ - z;

        int i1, i2;
        if (x > y) 
        {
            i1 = 1;
            j1 = 0;
        } 
        else 
        {
            i1 = 0;
            j1 = 1;
        }

        float x1 = x - i1 + UnskewFactor;
        float y1 = y - j1 + UnskewFactor;
        float x2 = x - 1 + 2 * UnskewFactor;
        float y2 = y - 1 + 2 * UnskewFactor;



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


