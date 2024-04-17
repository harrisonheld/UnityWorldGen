using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NUnit.Framework;
using WorldGenerator;
using UnityEngine.TestTools;

namespace UnitTests
{
    [TestFixture]
    public class BiomeTests
    {
        [Test]
        public void Biome_SetAndGetProperties()
        {
            Biome biome = new Biome();
            string biomeId = "testBiome";
            string biomeName = "Test Biome";
            HeightmapBase heightmap = new HeightmapFlat();
            Texture2D texture = new Texture2D(10, 10);
            Material skybox = new Material(Shader.Find("Skybox/Procedural"));

            biome.SetBiomeId(biomeId);
            biome.SetName(biomeName);
            biome.SetHeightMap(heightmap);
            biome.SetTexture(texture);
            biome.SetSkybox(skybox);

            Assert.AreEqual(biomeId, biome.GetBiomeId());
            Assert.AreEqual(biomeName, biome.GetName());
            Assert.AreEqual(heightmap, biome.GetHeightmap());
            Assert.AreEqual(texture, biome.GetTexture());
            Assert.AreEqual(skybox, biome.GetSkybox());
        }

        [Test]
        public void Biome_AddAndDeleteFeature()
        {
            Biome biome = new Biome();
            BiomeFeature feature = new BiomeFeature();
            feature.SetFeatureId("testFeature");

            biome.AddFeature(feature);
            Assert.Contains(feature, biome.GetFeatures());

            Assert.AreSame(feature, biome.GetFeature("testFeature"));

            biome.DeleteFeature("testFeature");
            Assert.IsEmpty(biome.GetFeatures());

            biome.SetFeatures(new List<BiomeFeature> { feature });
            Assert.Contains(feature, biome.GetFeatures());

            LogAssert.Expect(LogType.Warning, "Feature with ID NONEXISTANT not found.");
            biome.DeleteFeature("NONEXISTANT");
            LogAssert.Expect(LogType.Warning, "Feature with ID NONEXISTANT not found.");
            biome.GetFeature("NONEXISTANT");
        }

        [Test]
        public void Biome_ErrorHandling()
        {
            Biome biome = new Biome();

            LogAssert.Expect(LogType.Error, "The heightmap for this biome is null. Please assign one.");
            LogAssert.Expect(LogType.Error, "The texture for this biome is null. Please assign one.");
            LogAssert.Expect(LogType.Error, "The skybox for this biome is null. Please assign one.");
            Assert.AreEqual(biome.GetHeightmap(), null);
            Assert.AreEqual(biome.GetTexture(), null);
            Assert.AreEqual(biome.GetSkybox(), null);
        }
    }
}