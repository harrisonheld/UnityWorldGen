using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class BiomeFeature
{
    [SerializeField]
    [Tooltip("The name of the biome feature.")]
    public string _name;

    [SerializeField]
    [Tooltip("How often this feature will appear in the biome.")]
    [Range(0, 50)]
    public int frequency;

    [SerializeField]
    [Tooltip("The relative size of the features.")]
    [Range(0.0f, 10.0f)]
    public float _size;

    [SerializeField]
    [Tooltip("The feature's object model.")]
    public GameObject _model;

}