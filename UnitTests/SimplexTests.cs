using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;

public class SimplexTests
{
    [Test]
    public void TestHeightSameXYandSeed()
    {
        HeightmapSimplex heightmap = ScriptableObject.CreateInstance<HeightmapSimplex>();
        heightmap.SetSeed(1);
        float height1 = heightmap.GetHeight(1.0f, 1.0f);
        float height2 = heightmap.GetHeight(1.0f, 1.0f);
        Assert.AreEqual(height1, height2, "Heights with the same x, z, and seeds should be the same.");
    }

    [Test]
    public void TestHeightSameXYDifSeed()
    {
        HeightmapSimplex heightmap = ScriptableObject.CreateInstance<HeightmapSimplex>();
        heightmap.SetSeed(1);
        float height1 = heightmap.GetHeight(1.0f, 1.0f);
        heightmap.SetSeed(2);
        float height2 = heightmap.GetHeight(1.0f, 1.0f);
        Assert.AreNotEqual(height1, height2, "Heights with the same x and z, but different seeds should be different.");
    }
}
