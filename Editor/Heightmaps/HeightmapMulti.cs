using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

namespace WorldGenerator
{
    /// <summary>
    /// A heightmap which combines the influence of multiple other heightmaps.
    /// </summary>
    [CreateAssetMenu(fileName = "New Multi Heightmap", menuName = "WorldGenerator/Multi Heightmap")]
    public partial class HeightmapMulti : HeightmapBase
    {
        [field: SerializeField]
        [Tooltip("The list of all heightmaps to contribute to this heightmap.")]
        public List<HeightmapBase> Heightmaps { get; set; } = new();
        public override float GetHeight(float x, float z)
        {
            if(Heightmaps.Contains(this))
            {
                throw new InvalidOperationException("This Multi Heightmap contains itself.");
            }

            return Heightmaps.Sum(hm => hm.GetHeight(x, z));
        }
    }
}