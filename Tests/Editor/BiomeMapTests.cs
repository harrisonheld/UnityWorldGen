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
            BiomeMap map1 = new BiomeMap(worldSeed: 0, 3, 10.0f, 0, 0, 3);
            BiomeMap map2 = new BiomeMap(worldSeed: 1, 3, 10.0f, 0, 0, 3);

            BiomeWeight[] weights1 = map1.Sample(0, 0);
            BiomeWeight[] weights2 = map2.Sample(0, 0);

            Assert.AreNotEqual(weights1[0].Weight, weights2[0].Weight);
            Assert.AreNotEqual(weights1[1].Weight, weights2[1].Weight);
            Assert.AreNotEqual(weights1[2].Weight, weights2[2].Weight);
        }

        [Test]
        public void SameSeedSameValue()
        {
            BiomeMap map1 = new BiomeMap(worldSeed: 0, 3, 10.0f, 0, 0, 3);
            BiomeMap map2 = new BiomeMap(worldSeed: 0, 3, 10.0f, 0, 0, 3);

            BiomeWeight[] weights1 = map1.Sample(0, 0);
            BiomeWeight[] weights2 = map2.Sample(0, 0);

            Assert.AreEqual(weights1[0].BiomeIndex, weights2[0].BiomeIndex);
            Assert.AreEqual(weights1[1].BiomeIndex, weights2[1].BiomeIndex);
            Assert.AreEqual(weights1[2].BiomeIndex, weights2[2].BiomeIndex);
            Assert.AreEqual(weights1[0].Weight, weights2[0].Weight);
            Assert.AreEqual(weights1[1].Weight, weights2[1].Weight);
            Assert.AreEqual(weights1[2].Weight, weights2[2].Weight);
        }
    }
}