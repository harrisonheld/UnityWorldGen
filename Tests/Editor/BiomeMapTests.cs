using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NUnit.Framework;
using WorldGenerator;

namespace UnitTests
{
    [TestFixture]
    public class BiomeMapTests
    {
        [Test]
        public void DifferingSeedAltersValue()
        {
            List<Biome> biomes = new()
            {
                new Biome(),
                new Biome(),
                new Biome()
            };
            BiomeMap map1 = new BiomeMap(worldSeed: 0, biomes, 10.0f, 0, 0, 3);
            BiomeMap map2 = new BiomeMap(worldSeed: 1, biomes, 10.0f, 0, 0, 3);

            BiomeWeight[] weights1 = map1.Sample(0, 0);
            BiomeWeight[] weights2 = map2.Sample(0, 0);

            Assert.AreNotEqual(weights1[0].Weight, weights2[0].Weight);
            Assert.AreNotEqual(weights1[1].Weight, weights2[1].Weight);
            Assert.AreNotEqual(weights1[2].Weight, weights2[2].Weight);
        }

        [Test]
        public void SameSeedSameValue()
        {
            List<Biome> biomes = new()
            {
                new Biome(),
                new Biome(),
                new Biome()
            };
            BiomeMap map1 = new BiomeMap(worldSeed: 0, biomes, 10.0f, 0, 0, 3);
            BiomeMap map2 = new BiomeMap(worldSeed: 0, biomes, 10.0f, 0, 0, 3);

            BiomeWeight[] weights1 = map1.Sample(0, 0);
            BiomeWeight[] weights2 = map2.Sample(0, 0);

            Assert.AreEqual(weights1[0].BiomeIndex, weights2[0].BiomeIndex);
            Assert.AreEqual(weights1[1].BiomeIndex, weights2[1].BiomeIndex);
            Assert.AreEqual(weights1[2].BiomeIndex, weights2[2].BiomeIndex);
            Assert.AreEqual(weights1[0].Weight, weights2[0].Weight);
            Assert.AreEqual(weights1[1].Weight, weights2[1].Weight);
            Assert.AreEqual(weights1[2].Weight, weights2[2].Weight);
        }



        [Test]
        public void BiomeWeightsSumToOne()
        {
            List<Biome> biomes = new()
            {
                new Biome(),
                new Biome(),
                new Biome()
            };

            BiomeMap map = new BiomeMap(0, biomes, 10f, 0, 0, 10);

            for (float y = -5f; y < 5f; y += 0.1f)
            {
                for (float x = -5f; x < 5f; x += 0.1f)
                {
                    BiomeWeight[] weights = map.Sample(x, y);
                    float sum = 0;
                    for (int i = 0; i < weights.Length; i++)
                    {
                        sum += weights[i].Weight;
                    }
                    Assert.AreEqual(1, sum, 0.0001f);
                }
            }
        }

        [Test]
        public void ZeroWeightBiomeDoesNotAppear() {  
            
            List<Biome> biomes = new()
            {
                new Biome(),
                new Biome(),
                new Biome()
            };

            biomes[0].SetFrequencyWeight(100);
            biomes[1].SetFrequencyWeight(100);
            biomes[2].SetFrequencyWeight(0);
            BiomeMap map = new BiomeMap(0, biomes, 10f, 0, 0, 10);

            for(float y = -5f; y < 5f; y += 0.1f)
            {
                for(float x = -5f; x < 5f; x += 0.1f) {
                    BiomeWeight[] weights = map.Sample(x, y);
                    Assert.AreEqual(0, weights[2].Weight);
                }
            }
        }

        [Test]
        public void HugeWeightVersusTinyWeight()
        {
            List<Biome> biomes = new()
            {
                new Biome(),
                new Biome()
            };

            biomes[0].SetFrequencyWeight(10);
            biomes[1].SetFrequencyWeight(1000);

            BiomeMap map = new BiomeMap(0, biomes, 10f, 0, 0, 10);

            int tinyWeightBiomeCount = 0;
            int hugeWeightBiomeCount = 0;

            for (float y = -5f; y < 5f; y += 0.1f)
            {
                for (float x = -5f; x < 5f; x += 0.1f)
                {
                    BiomeWeight[] weights = map.Sample(x, y);
                    if (weights[0].Weight > weights[1].Weight)
                    {
                        tinyWeightBiomeCount++;
                    }
                    else
                    {
                        hugeWeightBiomeCount++;
                    }
                }
            }

            // since hugeWeightBiome has 100 times the weight of tinyWeightBiome, we expect to see it 100 times more often
            // but due to randomness, it may be a little more or less, so ill check more than 90
            Assert.Greater(hugeWeightBiomeCount, tinyWeightBiomeCount*90);
        }

        [Test]
        public void BiomeMapErrorHandling()
        {
            List<Biome> biomes = new()
            {
                new Biome(),
            };
            BiomeMap biomeMap = new BiomeMap(0, biomes, 10f, 0, 0, 10);

            // out of bounds
            Assert.Throws<System.ArgumentException>(() => biomeMap.Sample(-1000, -1000));
        }
    }
}