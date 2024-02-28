using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NUnit.Framework;
using WorldGenerator;

namespace UnitTests
{
    [TestFixture]

    /// <summary>
    /// In an ideal world, we would run statistical tests on MultiHash to ensure that it is a good hash function.
    /// These are simple tests to ensure that it is at least not completely broken.
    /// </summary>
    public class NumericalTests
    {
        [Test]
        public void MultiHashOrderMatters()
        {
            int hash1 = Helpers.MultiHash(0, 1);
            int hash2 = Helpers.MultiHash(1, 0);
            Assert.AreNotEqual(hash1, hash2);
        }

        [Test]
        public void MultiHashSameInputs()
        {
            int hash1 = Helpers.MultiHash(0, 0, 0);
            int hash2 = Helpers.MultiHash(0, 0, 0);
            Assert.AreEqual(hash1, hash2);
        }

        [Test]
        public void MultiHashDifferentInputsDifferentResults()
        {
            int hash1 = Helpers.MultiHash(0, 0, 0);
            int hash2 = Helpers.MultiHash(1, 1, 1);
            Assert.AreNotEqual(hash1, hash2);
        }

        [Test]
        public void MultiHashDummyValueWorks()
        {
            // if you want to ensure that your results are entirely unique, you can throw a dummy value at the end
            // for instance, we do a lot of MultiHash(x, y) when iterating over coordinates.
            // we may want different results for each context, so you can throw the string "biomemap" or "simplexheightmap" at the end
            // to ensure that the results are unique to that context. it doesn't really matter for now, but its a trick you can use.
            int hash1 = Helpers.MultiHash(1, 2, 3, "first");
            int hash2 = Helpers.MultiHash(1, 2, 3, "second");
            Assert.AreNotEqual(hash1, hash2);
        }
    }
}