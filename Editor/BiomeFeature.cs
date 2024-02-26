using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class BiomeFeature
{
    [SerializeField]
    [Tooltip("The name of the biome feature.")]
    public string Name;

    [SerializeField]
    [Tooltip("How often this feature will appear in the biome.")]
    [Range(0, 100)]
    public int Frequency;

    [SerializeField]
    [Tooltip("The relative size of the features.")]
    [Range(0.0f, 10.0f)]
    public float Size;

    [SerializeField]
    [Tooltip("The GameObject to spawn.")]
    public GameObject Prefab;

}