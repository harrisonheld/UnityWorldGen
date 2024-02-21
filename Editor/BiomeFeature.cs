using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class BiomeFeature
{
    [SerializeField]
    [Tooltip("The name of the biome feature.")]
    private string _name;

    [SerializeField]
    [Tooltip("How often this feature will appear in the biome.")]
    [Range(0, 50)]
    private int _frequency;

    [SerializeField]
    [Tooltip("The relative size of the features.")]
    [Range(0.0f, 10.0f)]
    private float _size;

    [SerializeField]
    [Tooltip("The feature's mesh model.")]
    private Mesh _model;

}