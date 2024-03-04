using System;
using UnityEngine;

namespace WorldGenerator
{
    /// <summary>
    /// This is the base class from which all Heightmaps derive.
    /// These are idealized surfaces. This class is not concerned about resolution, or tris/quads, or anything like that.
    /// You can sample it at ANY worldX and worldZ, and it will return a height.
    /// </summary>
    [Serializable]
    public abstract class HeightmapBase : ScriptableObject
    {
        /// <summary>
        /// Returns the height at the given world x and z.
        /// </summary>
        public abstract float GetHeight(float x, float z);
        /// <summary>
        /// For heightmaps with an element of randomness, this method sets the seed.
        /// </summary>
        public virtual void SetSeed(int seed) { }
    }
}