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
    [Tooltip("The scale of the features.")]
    public Vector3 Scale = new Vector3(1f, 1f, 1f);

    [SerializeField]
    [Tooltip("Whether or not the object should be normal to the terrain.")]
    public bool SetNormal;

    [SerializeField]
    [Tooltip("The GameObject to spawn.")]
    public GameObject Prefab;

}