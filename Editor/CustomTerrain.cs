using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

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

            string[] preset_biome_options = new string[] { "Desert", "Hills", "Plains", "Mountain", "Valley", "Custom" };
            Dictionary<string, (string heightmap, string texture)> biomePresets = new Dictionary<string, (string, string)>
            {
                { "Desert", ("Desert_Heightmap", "Sand") },
                { "Hills", ("Hills_Heightmap", "Grass") },
                { "Plains", ("Plains_Heightmap", "Grass") },
                { "Mountain", ("Mountain_Heightmap", "Stone") },
                { "Valley", ("Valley_Heightmap", "Grass") },
                { "Custom", ("Flat0", "Grass") }
            };
            selected_biome_preset_index = EditorGUILayout.Popup("New Biome", selected_biome_preset_index, preset_biome_options);

            if (GUILayout.Button("Add Biome"))
            {
                Biome newBiome = new();

                if (biomePresets.TryGetValue(preset_biome_options[selected_biome_preset_index], out var preset))
                {
                    newBiome.SetHeightMap(Resources.Load(preset.heightmap, typeof(HeightmapBase)) as HeightmapBase);
                    newBiome.SetTexture(Resources.Load(preset.texture, typeof(Texture2D)) as Texture2D);
                    terrain.AddBiome(newBiome);
                }
                else
                {
                    Debug.LogError("Unrecognized Option");
                }
            }

            if (GUILayout.Button("Generate Terrain"))
            {
                terrain.GenerateTerrain();
            }
            
        }
    }
#endif

    [Tooltip("The size of each chunk in world units. This is the number of units along each side of the chunk.")]
    [SerializeField] private int _chunkSize = 50;
    [Tooltip("The resolution of each chunk. This is the number of vertices along each side of the chunk.")]
    [SerializeField] private int _chunkResolution = 250;

    [Tooltip("The seed string used to generate the terrain. If left empty, a random seed will be used.")]
    [SerializeField] private string _worldSeedString = "";
    private int _worldSeed;
    [SerializeField] private List<Biome> _biomes = new();

    const int TEX_SIZE = 512;

    private Material multitextureMat;

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

        // make material
        this.multitextureMat = new Material(Shader.Find("Custom/MultiTexture"));
        Texture2DArray textureArray = new Texture2DArray(TEX_SIZE, TEX_SIZE, _biomes.Count, TextureFormat.RGBA32, true);
        textureArray.filterMode = FilterMode.Bilinear;
        textureArray.wrapMode = TextureWrapMode.Repeat;
        for (int i = 0; i < _biomes.Count; i++)
        {
            // get tex
            Texture2D originalTex = _biomes[i].GetTexture();
            // copy to a readable texture
            Texture2D readableTex = new Texture2D(originalTex.width, originalTex.height, originalTex.format, originalTex.mipmapCount > 1);
            Graphics.CopyTexture(originalTex, readableTex);
            // scale it to the correct size
            RenderTexture rt = new RenderTexture(TEX_SIZE, TEX_SIZE, 0);
            RenderTexture.active = rt;
            Graphics.Blit(readableTex, rt);
            Texture2D scaledTex = new Texture2D(TEX_SIZE, TEX_SIZE);
            scaledTex.ReadPixels(new Rect(0, 0, TEX_SIZE, TEX_SIZE), 0, 0);
            scaledTex.Apply();
            // send to shader array
            Color[] pixels = scaledTex.GetPixels();
            textureArray.SetPixels(pixels, i);
        }
        textureArray.Apply();
        multitextureMat.SetTexture($"_TextureArray", textureArray);

        // seed
        if (_worldSeedString == "")
        {
            _worldSeed = ((int)DateTime.Now.Ticks);
        }
        else
        {
            _worldSeed = Helpers.MultiHash(_worldSeedString);
        }
        for(int i = 0; i < _biomes.Count; i++)
        {
            _biomes[i].GetHeightmap().SetSeed(_worldSeed);
        }

        // chunk
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
            worldSeed: _worldSeed,
            biomeCount: _biomes.Count,
            chunkSize: _chunkSize,
            chunkX: chunkX,
            chunkZ: chunkZ
        );

        // Vertices
        Vector3[] vertices = new Vector3[_chunkResolution * _chunkResolution];
        Vector2[] uvs = new Vector2[vertices.Length];
        Vector2[] uv2s = new Vector2[vertices.Length];
        for (int x = 0; x < _chunkResolution; x++)
        {
            for (int z = 0; z < _chunkResolution; z++)
            {
                int i = x * _chunkResolution + z;

                float u = x / (float)(_chunkResolution - 1);
                float v = z / (float)(_chunkResolution - 1);
                uvs[i] = new Vector2(u, v);
                float offsetX = u * _chunkSize - _chunkSize / 2;
                float offsetZ = v * _chunkSize - _chunkSize / 2;

                // get biome
                int biomeIdx = biomeMap.Sample(offsetX, offsetZ);
                Biome biome = _biomes[biomeIdx];

                // set height
                float worldX = (chunkX * _chunkSize) + offsetX;
                float worldZ = (chunkZ * _chunkSize) + offsetZ;
                float height = biome.GetHeightmap().GetHeight(worldX, worldZ);
                vertices[i] = new Vector3(offsetX, height, offsetZ);

                // put the texture index in the uv2.x
                int textureIdx = biomeIdx;
                uv2s[i] = new Vector2(textureIdx, 0);
            }
        }
        mesh.vertices = vertices;
        mesh.uv = uvs;
        mesh.uv2 = uv2s;

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
        // set mat
        chunk.GetComponent<MeshRenderer>().sharedMaterial = multitextureMat;
        // add as child
        chunk.transform.parent = this.transform;
        // set the position
        chunk.transform.position = new Vector3(chunkX * _chunkSize, 0, chunkZ * _chunkSize);

        // TODO: Adding features to each biome in the chunk
        //
        // for each biome in biome map:
        //     get the x and z bounds of the biome
        //     for each feature in the biome:
        //         for loop (based on frequency):
        //             feature_x = random number based on bounds of x
        //             feature_z = random number based on bounds of z
        //             feature_y = use heightmap at (feature_x, feature_y)
        //             create the object at that coordinate
        //             set object's parent as chunk
        //
        // OR
        //
        // for each vertex:
        //     get the biome at that vertex
        //     for feature in biome: go in order from lowest frequency to hgihest so that less freqent things get a chance to show up first (tree vs grass)
        //         probability of showing up = frequency / 100 or something
        //         if it shows up:
        //             place object at that vertex's coords
        //             set object's parent as chunk
        //             move on to next vertex
        //
        // attempting the second solution below

        int chunkSeed = Helpers.MultiHash(_worldSeed, chunkX, chunkZ);
        System.Random rand = new System.Random(chunkSeed);

        for (int i = 0; i < vertices.Length; i++)
        {
            Vector3 vertex = vertices[i]; // (offsetX, height, offsetZ)
            int biomeIndex = (int) uv2s[i].x;
            Biome biome = _biomes[biomeIndex];
            for (int j = 0; j < biome.GetFeatures().Count; j++)
            {
                BiomeFeature feature = biome.GetFeatures()[j];
                double randomProbability = rand.NextDouble();
                // double featureProbability = feature.Frequency / 10000.0f;
                // double featureProbability = feature.Frequency * Math.Log(feature.Frequency + 1.0f) / 10000.0f;
                double featureProbability = 0.1f * Convert.ToInt32(feature.Frequency!=0) * (1f / (1f + Mathf.Exp(-((feature.Frequency*0.75f-50f)) / 5f)));
                if (randomProbability < featureProbability)
                {
                    if (feature.Prefab != null)
                    {
                        GameObject spawnedObject = Instantiate(feature.Prefab);
                        // move to chunk position
                        spawnedObject.transform.position = new Vector3(chunkX, 0, chunkZ) * _chunkSize;
                        // set position within chunk
                        spawnedObject.transform.position += vertex;
                        spawnedObject.transform.rotation = Quaternion.Euler(0, rand.Next(0, 360), 0);
                        if (feature.SetNormal)
                        {
                            // make normal to terrain
                            Vector3 normal = mesh.normals[i];
                            spawnedObject.transform.rotation = Quaternion.FromToRotation(Vector3.up, normal);
                        }
                        // set parent as chunk
                        spawnedObject.transform.parent = chunk.transform;
                    }
                    else
                    {
                        Debug.LogError("Prefab to spawn is not assigned!");
                    }

                    break; // stop adding objects to this vertex once one has been added
                }
            }
        }
    }
}