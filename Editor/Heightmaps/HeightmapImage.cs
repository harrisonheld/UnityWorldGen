using UnityEngine;
using System;

namespace WorldGenerator
{
    /// <summary>
    /// A heightmap based off an image. Pixels of heigher value will be higher in elevation.
    /// </summary>
    [CreateAssetMenu(fileName = "New Image Heightmap", menuName = "WorldGenerator/Image Heightmap")]
    public partial class HeightmapImage : HeightmapBase
    {
        [field: SerializeField]
        [Tooltip("The height that the brightest pixel will be at.")]
        public float MaxHeight { get; set; } = 10.0f;

        [field: SerializeField]
        [Tooltip("The height that the darkest pixel will be at.")]
        public float MinHeight { get; set; } = 0.0f;

        [field: SerializeField]
        [Tooltip("The scale of the texture.")]
        public float TextureScale { get; set; } = 1.0f;

        [field: SerializeField]
        [Tooltip("The image to use as the heightmap.")]
        public Texture2D Image { get; set; }

        private void OnEnable()
        {
            if (Image == null)
                return;

            if(Image.isReadable)
                return;

            // make sure the texture is readable
            Texture2D readableTex = new Texture2D(Image.width, Image.height, Image.format, Image.mipmapCount > 1);
            Graphics.CopyTexture(Image, readableTex);
            Image = readableTex;
        }

        public override float GetHeight(float worldx, float worldz)
        {
            if(Image == null)
            {
                throw new ArgumentNullException("Image is null. Please assign a texture to the heightmap.");
            }
            int x = (int)(worldx * TextureScale);
            int z = (int)(worldz * TextureScale);
            x = x % Image.width;
            z = z % Image.height;
            float value = Image.GetPixel(x, z).grayscale;
            float lerped = Mathf.Lerp(MinHeight, MaxHeight, value);
            return lerped;
        }
    }

}