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
        public void MultiHashDunderValueWorks()
        {
            int hash1 = Helpers.MultiHash(1, 2, 3, "first");
            int hash2 = Helpers.MultiHash(1, 2, 3, "second");
            Assert.AreNotEqual(hash1, hash2);
        }
    }
}