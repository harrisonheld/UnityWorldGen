using System;
using System.Collections.Generic;
using UnityEngine;

namespace WorldGenerator
{

    [Serializable]
    public class BiomeFeature
    {
        [SerializeField]
        private string _featureId;

        [SerializeField]
        [Tooltip("The name of the biome feature.")]
        public string Name;

        [SerializeField]
        [Tooltip("How often this feature will appear in the biome.")]
        [Range(0, 100)]
        public int Frequency = 15;

        [SerializeField]
        [Tooltip("The scale of the features.")]
        public Vector3 Scale = new Vector3(1f, 1f, 1f);

        [SerializeField]
        [Tooltip("Whether or not the object should be normal to the terrain.")]
        public bool SetNormal;

        [SerializeField]
        [Tooltip("The GameObject to spawn.")]
        public GameObject Prefab;

        public string GetFeatureId()
        {
            return _featureId;
        }

        public void SetFeatureId(string featureId)
        {
            _featureId = featureId;
        }

    }
}