using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NUnit.Framework;
using WorldGenerator;

namespace UnitTests
{
    [TestFixture]

    public class HeightmapTests
    {
        [Test] 
        public void HeightmapFlatTest()
        {
            HeightmapFlat heightmap = ScriptableObject.CreateInstance<HeightmapFlat>();

            heightmap.Height = 0;
            Assert.AreEqual(0, heightmap.GetHeight(0f, 0f));
            Assert.AreEqual(0, heightmap.GetHeight(1f, 1f));
            Assert.AreEqual(0, heightmap.GetHeight(100f, 100f));
            Assert.AreEqual(0, heightmap.GetHeight(-100f, -100f));

            heightmap.Height = 42;
            Assert.AreEqual(42, heightmap.GetHeight(0f, 0f));
            Assert.AreEqual(42, heightmap.GetHeight(1f, 1f));
            Assert.AreEqual(42, heightmap.GetHeight(100f, 100f));
            Assert.AreEqual(42, heightmap.GetHeight(-100f, -100f));

            Object.DestroyImmediate(heightmap);
        }

        [Test]
        public void HeightmapSinusoidalTest()
        {
            HeightmapSinusoidal heightmap = ScriptableObject.CreateInstance<HeightmapSinusoidal>();

            heightmap.Amplitude = 1.0f;
            heightmap.Scale = 1.0f;

            float tolerance = 0.001f;

            // values for sin being 0
            Assert.AreEqual(0, heightmap.GetHeight(0f, 0f), tolerance);
            Assert.AreEqual(0, heightmap.GetHeight(Mathf.PI, Mathf.PI), tolerance);
            Assert.AreEqual(0, heightmap.GetHeight(42 * Mathf.PI, 42 * Mathf.PI), tolerance);
            Assert.AreEqual(0, heightmap.GetHeight(-42 * Mathf.PI, -42 * Mathf.PI), tolerance);
            Assert.AreEqual(0, heightmap.GetHeight(42 * Mathf.PI, -42 * Mathf.PI), tolerance);
            Assert.AreEqual(0, heightmap.GetHeight(-42 * Mathf.PI, 42 * Mathf.PI), tolerance);

            // values for sin being 1
            Assert.AreEqual(1, heightmap.GetHeight(Mathf.PI / 2, Mathf.PI / 2), tolerance);
            Assert.AreEqual(1, heightmap.GetHeight(3 * Mathf.PI / 2, 3 * Mathf.PI / 2), tolerance);
            Assert.AreEqual(1, heightmap.GetHeight(5 * Mathf.PI / 2, 5 * Mathf.PI / 2), tolerance);
            Assert.AreEqual(1, heightmap.GetHeight(-Mathf.PI / 2, -Mathf.PI / 2), tolerance);
            Assert.AreEqual(1, heightmap.GetHeight(-3 * Mathf.PI / 2, -3 * Mathf.PI / 2), tolerance);
            Assert.AreEqual(1, heightmap.GetHeight(-5 * Mathf.PI / 2, -5 * Mathf.PI / 2), tolerance);

            // values for sin being -1
            Assert.AreEqual(-1, heightmap.GetHeight(-Mathf.PI/2, Mathf.PI/2), tolerance);
            Assert.AreEqual(-1, heightmap.GetHeight(3*Mathf.PI/2, Mathf.PI/2), tolerance);
            Assert.AreEqual(-1, heightmap.GetHeight(Mathf.PI/2, - Mathf.PI/2), tolerance);

            // various values
            Assert.AreEqual(0.285960729313, heightmap.GetHeight(20f, 82f), tolerance);
            Assert.AreEqual(0.285960729313, heightmap.GetHeight(82f, 20f), tolerance);
            Assert.AreEqual(0.134193818057, heightmap.GetHeight(2.4f, 0.2f), tolerance);
            Assert.AreEqual(0.134193818057, heightmap.GetHeight(0.2f, 2.4f), tolerance);

            Object.DestroyImmediate(heightmap);
        }

        [Test]
        public void HeightmapPerlinTest()
        {
            HeightmapPerlin heightmap = ScriptableObject.CreateInstance<HeightmapPerlin>();

            // seed should matter
            heightmap.SetSeed(0);
            float val1 = heightmap.GetHeight(0f, 0f);
            heightmap.SetSeed(1);
            float val2 = heightmap.GetHeight(0f, 0f);
            Assert.AreNotEqual(val1, val2);

            Object.DestroyImmediate(heightmap);
        }

        [Test]
        public void HeightmapSimplexTest()
        {
            HeightmapSimplex heightmap = ScriptableObject.CreateInstance<HeightmapSimplex>();

            // seed should matter
            heightmap.SetSeed(0);
            float val1 = heightmap.GetHeight(0f, 0f);
            heightmap.SetSeed(1);
            float val2 = heightmap.GetHeight(0f, 0f);
            Assert.AreNotEqual(val1, val2);

            Object.DestroyImmediate(heightmap);
        }

        [Test]
        public void HeightmapImageTest()
        {
            HeightmapImage heightmap = ScriptableObject.CreateInstance<HeightmapImage>();

            heightmap.MinHeight = 0.0f;
            heightmap.MaxHeight = 25.0f;
            heightmap.TextureScale = 1.0f;
            heightmap.Image = new Texture2D(2, 2);
            heightmap.Image.SetPixel(0, 0, new Color(0, 0, 0));
            heightmap.Image.SetPixel(0, 1, new Color(1, 1, 1));
            heightmap.Image.SetPixel(1, 0, new Color(0.5f, 0.5f, 0.5f));
            heightmap.Image.SetPixel(1, 1, new Color(0.25f, 0.25f, 0.25f));
            heightmap.Image.Apply();

            float tolerance = 0.05f;
            Assert.AreEqual(0, heightmap.GetHeight(0f, 0f), tolerance);
            Assert.AreEqual(25f, heightmap.GetHeight(0f, 1f), tolerance);
            Assert.AreEqual(25f * 0.5f, heightmap.GetHeight(1f, 0f), tolerance);
            Assert.AreEqual(25f * 0.25f, heightmap.GetHeight(1f, 1f), tolerance);

            Object.DestroyImmediate(heightmap);
        }

        [Test]
        public void HeightmapImageNullTest()
        {
            HeightmapImage heightmap = ScriptableObject.CreateInstance<HeightmapImage>();
            Assert.Throws<System.ArgumentNullException>(() => heightmap.GetHeight(0f, 0f));
            Object.DestroyImmediate(heightmap);
        }

        [Test]
        public void MultiHeightmapTest()
        {
            // constituent heightmaps
            HeightmapFlat heightmap1 = ScriptableObject.CreateInstance<HeightmapFlat>();
            heightmap1.Height = 3;
            HeightmapFlat heightmap2 = ScriptableObject.CreateInstance<HeightmapFlat>();
            heightmap2.Height = 23;

            // multi heightmap to be tested
            HeightmapMulti heightmap = ScriptableObject.CreateInstance<HeightmapMulti>();
            heightmap.Heightmaps = new List<HeightmapBase> { heightmap1, heightmap2 };

            // test
            Assert.AreEqual(26, heightmap.GetHeight(0f, 0f));
            Assert.AreEqual(26, heightmap.GetHeight(1f, 1f));
            Assert.AreEqual(26, heightmap.GetHeight(100f, 100f));
            Assert.AreEqual(26, heightmap.GetHeight(-100f, -100f));


            Object.DestroyImmediate(heightmap1);
            Object.DestroyImmediate(heightmap2);
            Object.DestroyImmediate(heightmap);
        }

        [Test]
        public void MultiHeightmapCircularReference()
        {
            // a heightmap containing itself should throw an error
            HeightmapMulti heightmap = ScriptableObject.CreateInstance<HeightmapMulti>();
            heightmap.Heightmaps = new List<HeightmapBase> { heightmap };
            Assert.Throws<System.InvalidOperationException>(() => heightmap.GetHeight(0f, 0f));

            Object.DestroyImmediate(heightmap);
        }
    }
}