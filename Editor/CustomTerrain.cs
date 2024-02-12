using System;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteInEditMode]
public class CustomTerrain : MonoBehaviour
{
    [SerializeField] private int _width = 50;
    [SerializeField] private int _height = 50;
    [SerializeField] private int _resolution = 250;

    [Tooltip("The seed string used to generate the terrain. If left empty, a random seed will be used.")]
    [SerializeField] private string _worldSeedString = "";
    [SerializeField] private List<Biome> _biomes;

#if UNITY_EDITOR
    [CustomEditor(typeof(CustomTerrain))]
    public class CustomTerrainEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            CustomTerrain terrain = (CustomTerrain)this.target;

            GUILayout.Space(10);

            if (GUILayout.Button("Generate Terrain"))
            {
                terrain.GenerateTerrain();
            }
        }
    }
#endif

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

        Biome biome = _biomes[0];

        // generate a biome seed. this should be some function of the world seed, and the biome's XY position
        float biomeX = 0;
        float biomeY = 0;
        int hash = Helpers.MultiHash(worldSeed, biomeX, biomeY);
        biome.GetHeightmap().SetSeed(hash);

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

        // add biome material
        GetComponent<MeshRenderer>().sharedMaterial = biome.GetMaterial();
    }

}
