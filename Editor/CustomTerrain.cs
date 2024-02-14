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

    [SerializeField] private int _chunkSize = 50;
    [SerializeField] private int _chunkResolution = 250;

    [Tooltip("The scale of the biome map. Make this large to make each biome bigger.")]
    [SerializeField] private float _biomeSize = 1.0f;

    [Tooltip("The seed string used to generate the terrain. If left empty, a random seed will be used.")]
    [SerializeField] private string _worldSeedString = "";
    [SerializeField] private List<Biome> _biomes;

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

        // warnings
        if(this.transform.position != Vector3.zero)
        {
            Debug.LogWarning("The terrain is not at the origin. This may cause issues.");
        }
        if(this.transform.rotation != Quaternion.identity)
        {
            Debug.LogWarning("The terrain has a non-default rotation. This may cause issues.");
        }
        if(this.transform.localScale != Vector3.one)
        {
            Debug.LogWarning("The terrain has a non-default scale. This may cause issues.");
        }

        // clear the children
        for(int i = this.transform.childCount - 1; i >= 0; i--)
        {
            GameObject.DestroyImmediate(this.transform.GetChild(i).gameObject);
        }

        // chunky
        for(int x = -1; x <= 1; x++)
        {
            for(int z = -1; z <= 1; z++)
            {
                GenerateChunk(x, z);
            }
        }
    }

    public void GenerateChunk(int chunkX, int chunkZ)
    {
        Mesh mesh = new Mesh();
        mesh.name = $"Chunk Mesh ({chunkX}, {chunkZ})";

        BiomeMap biomeMap = new(
            worldSeed: Helpers.MultiHash(_worldSeedString),
            biomeCount: _biomes.Count,
            chunkSize: _chunkSize,
            chunkX: chunkX,
            chunkZ: chunkZ
        );

        // Vertices
        Vector3[] vertices = new Vector3[_chunkResolution * _chunkResolution];
        Vector2[] uvs = new Vector2[vertices.Length];
        for (int x = 0; x < _chunkResolution; x++)
        {
            for (int z = 0; z < _chunkResolution; z++)
            {
                float u = x / (float)(_chunkResolution - 1);
                float v = z / (float)(_chunkResolution - 1);
                uvs[x * _chunkResolution + z] = new Vector2(u, v);
                float worldX = u * _chunkSize - _chunkSize / 2;
                float worldZ = v * _chunkSize - _chunkSize / 2;

                // get biome
                int biomeIdx = biomeMap.Sample(worldX, worldZ);
                Biome biome = _biomes[biomeIdx];

                float height = biome.GetHeightmap().GetHeight(worldX, worldZ);
                vertices[x * _chunkResolution + z] = new Vector3(worldX, height, worldZ);
            }
        }
        mesh.vertices = vertices;
        mesh.uv = uvs;

        // Triangles
        int[] triangles = new int[(_chunkResolution - 1) * (_chunkResolution - 1) * 6];
        int triangleIndex = 0;
        for (int x = 0; x < _chunkResolution - 1; x++)
        {
            for (int z = 0; z < _chunkResolution - 1; z++)
            {
                int vertexIndex = x * _chunkResolution + z;
                triangles[triangleIndex] = vertexIndex;
                triangles[triangleIndex + 1] = vertexIndex + 1;
                triangles[triangleIndex + 2] = vertexIndex + _chunkResolution;
                triangles[triangleIndex + 3] = vertexIndex + 1;
                triangles[triangleIndex + 4] = vertexIndex + _chunkResolution + 1;
                triangles[triangleIndex + 5] = vertexIndex + _chunkResolution;

                triangleIndex += 6;
            }
        }
        mesh.triangles = triangles;

        // Normals
        mesh.RecalculateNormals();

        // add all components necessary for rendering a mesh
        GameObject chunk = new GameObject($"Chunk ({chunkX}, {chunkZ})");
        chunk.AddComponent<MeshFilter>();
        chunk.AddComponent<MeshRenderer>();
        chunk.AddComponent<MeshCollider>();
        // set the meshes
        chunk.GetComponent<MeshFilter>().sharedMesh = mesh;
        chunk.GetComponent<MeshCollider>().sharedMesh = mesh;
        // set the materials
        chunk.GetComponent<MeshRenderer>().material = _biomes[0].GetMaterial();
        // add as child
        chunk.transform.parent = this.transform;
        // set the position
        chunk.transform.position = new Vector3(chunkX * _chunkSize, 0, chunkZ * _chunkSize);
    }
}
