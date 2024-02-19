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
    private int _frequency;

    [SerializeField]
    [Tooltip("The relative size of the features.")]
    private float _size;

}