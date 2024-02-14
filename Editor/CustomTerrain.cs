using System;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteInEditMode]
public class CustomTerrain : MonoBehaviour
{
#if UNITY_EDITOR
    [CustomEditor(typeof(CustomTerrain))]
    public class CustomTerrainEditor : Editor
    {
        int selected_biome_preset_index = 0;

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            CustomTerrain terrain = (CustomTerrain)this.target;

            GUILayout.Space(10);

            // START of adding biomes **********

            // create pop up with options
            string[] preset_biome_options = new string[]
            {
                "Desert", "Hills", "Plains", "Mountain", "Valley", "Custom"
            };
            selected_biome_preset_index = EditorGUILayout.Popup("New Biome", selected_biome_preset_index, preset_biome_options);

            // use selected option from pop up and add to list of biomes
            if (GUILayout.Button("Add Biome"))
            {
                Biome newBiome = new();

                switch (selected_biome_preset_index)
                {
                    case 0: // desert
                        newBiome.SetHeightMap(Resources.Load("Desert_Heightmap", typeof(HeightmapBase)) as HeightmapBase);
                        newBiome.SetMaterial(Resources.Load("Sand", typeof(Material)) as Material);
                        break;
                    case 1: // hills
                        newBiome.SetHeightMap(Resources.Load("Hills_Heightmap", typeof(HeightmapBase)) as HeightmapBase);
                        newBiome.SetMaterial(Resources.Load("Grass", typeof(Material)) as Material);
                        break;
                    case 2: // plains
                        newBiome.SetHeightMap(Resources.Load("Plains_Heightmap", typeof(HeightmapBase)) as HeightmapBase);
                        newBiome.SetMaterial(Resources.Load("Grass", typeof(Material)) as Material);
                        break;
                    case 3: // mountain
                        newBiome.SetHeightMap(Resources.Load("Mountain_Heightmap", typeof(HeightmapBase)) as HeightmapBase);
                        newBiome.SetMaterial(Resources.Load("Stone", typeof(Material)) as Material);
                        break;
                    case 4: // valley
                        newBiome.SetHeightMap(Resources.Load("Valley_Heightmap", typeof(HeightmapBase)) as HeightmapBase);
                        newBiome.SetMaterial(Resources.Load("Grass", typeof(Material)) as Material);
                        break;
                    case 5: // custom
                        newBiome.SetHeightMap(Resources.Load("Flat0", typeof(HeightmapBase)) as HeightmapBase);
                        newBiome.SetMaterial(Resources.Load("Grass", typeof(Material)) as Material);
                        break;
                    default:
                        Debug.LogError("Unrecognized Option");
                        break;
                }

                terrain.AddBiome(newBiome);
            }
            // END of adding biomes ***********

            if (GUILayout.Button("Generate Terrain"))
            {
                terrain.GenerateTerrain();
            }
            
        }
    }
#endif

    [SerializeField] private int _width = 50;
    [SerializeField] private int _height = 50;
    [SerializeField] private int _resolution = 250;

    [Tooltip("The seed string used to generate the terrain. If left empty, a random seed will be used.")]
    [SerializeField] private string _worldSeedString = "";
    [SerializeField] private List<Biome> _biomes;

    [Tooltip("The scale of the biome map. Make this large to make each biome bigger.")]
    [SerializeField] private float _biomeSize = 1.0f;

    public void AddBiome(Biome newBiome)
    {
        this._biomes.Add(newBiome);
    }
    public void GenerateTerrain()
    {
        // errors
        if(_biomes.Count == 0)
        {
            Debug.LogError("Cannot generate terrain because no biomes have been added to the terrain.");
            return;
        }

        // seed
        int worldSeed;
        if(string.IsNullOrEmpty(_worldSeedString))
        {
            worldSeed = DateTime.UtcNow.Ticks.GetHashCode();
        }
        else
        {
            worldSeed = _worldSeedString.GetHashCode();
        }
        // set up biome map
        BiomeMap biomeMap = new();
        biomeMap.SetSeed(worldSeed);
        biomeMap.SetBiomeCount(_biomes.Count);
        biomeMap.SetBiomeSize(_biomeSize);

        Mesh mesh = new Mesh();
        mesh.name = "TerrainMesh";

        // Vertices
        Vector3[] vertices = new Vector3[_resolution * _resolution];
        for (int x = 0; x < _resolution; x++)
        {
            for (int z = 0; z < _resolution; z++)
            {
                float normalizedX = x / (float)(_resolution - 1);
                float normalizedZ = z / (float)(_resolution - 1);
                float worldX = normalizedX * _width - _width / 2;
                float worldZ = normalizedZ * _height - _height / 2;

                // get biome
                Biome biome = _biomes[biomeMap.Sample(worldX, worldZ)];

                float height = biome.GetHeightmap().GetHeight(worldX, worldZ);
                vertices[x * _resolution + z] = new Vector3(worldX, height, worldZ);
            }
        }
        mesh.vertices = vertices;

        // Triangles
        int[] triangles = new int[(_resolution - 1) * (_resolution - 1) * 6];
        int triangleIndex = 0;
        for (int x = 0; x < _resolution - 1; x++)
        {
            for (int z = 0; z < _resolution - 1; z++)
            {
                int vertexIndex = x * _resolution + z;
                triangles[triangleIndex] = vertexIndex;
                triangles[triangleIndex + 1] = vertexIndex + 1;
                triangles[triangleIndex + 2] = vertexIndex + _resolution;
                triangles[triangleIndex + 3] = vertexIndex + 1;
                triangles[triangleIndex + 4] = vertexIndex + _resolution + 1;
                triangles[triangleIndex + 5] = vertexIndex + _resolution;

                triangleIndex += 6;
            }
        }
        mesh.triangles = triangles;

        // Normals
        mesh.RecalculateNormals();

        // UVs - for textures
        Vector2[] uvs = new Vector2[vertices.Length];
        for (int i = 0; i < uvs.Length; i++)
        {
            uvs[i] = new Vector2(vertices[i].x / _resolution, vertices[i].z / _resolution);
        }
        mesh.uv = uvs;

        // add all components necessary for rendering a mesh
        if(GetComponent<MeshFilter>() == null)
        {
            this.gameObject.AddComponent<MeshFilter>();
        }
        if (GetComponent<MeshRenderer>() == null)
        {
            this.gameObject.AddComponent<MeshRenderer>();
        }
        if (GetComponent<MeshCollider>() == null)
        {
            this.gameObject.AddComponent<MeshCollider>();
        }
        // set the meshes
        GetComponent<MeshFilter>().sharedMesh = mesh;
        GetComponent<MeshCollider>().sharedMesh = mesh;
    }

}
