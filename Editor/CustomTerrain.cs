using System;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace WorldGenerator
{

    /// <summary>
    /// This is our Unity component. Think of it as the entry point or 'main' of our project.
    /// By serializing fields here, they will be visible in the Unity Editor.
    /// </summary>
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
                    { "Desert", ("DesertHeightmap", "Sand") },
                    { "Hills", ("HillsHeightmap", "Grass") },
                    { "Plains", ("plains_simplex_heightmap", "Grass") },
                    { "Mountain", ("MountainHeightmap", "Stone") },
                    { "Valley", ("valley_simplex_heightmap", "Grass") },
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

        [Tooltip("The seed string used to generate the terrain. If left empty, a random seed will be used.")]
        [SerializeField] private string _worldSeedString = "";

        [Tooltip("The size of each chunk in world units. This is the number of units along each side of the chunk.")]
        [SerializeField] private int _chunkSize = 50;
        [Tooltip("The resolution of each chunk. This is the number of vertices along each side of the chunk.")]
        [SerializeField] private int _chunkResolution = 250;

        [Tooltip("The number of biomes that will be placed per chunk. Increasing this will generally make your biomes smaller.")]
        [Range(1, 10)]
        [SerializeField] private int _biomesPerChunk = 3;
        [Tooltip("The biomes that will be used to generate the terrain.")]
        [SerializeField] private List<Biome> _biomes = new();

        private int _worldSeed;

        const int TEX_SIZE = 512;
        private Material _multitextureMat;

        private GameObject[,] _chunks;

        public void AddBiome(Biome newBiome)
        {
            this._biomes.Add(newBiome);
        }
        public void GenerateTerrain()
        {
            // errors
            if (_biomes.Count == 0)
            {
                Debug.LogError("Cannot generate terrain because no biomes have been added to the terrain.");
                return;
            }

            // warnings
            if (this.transform.position != Vector3.zero)
            {
                Debug.LogWarning("The terrain is not at the origin. This may cause issues.");
            }
            if (this.transform.rotation != Quaternion.identity)
            {
                Debug.LogWarning("The terrain has a non-default rotation. This may cause issues.");
            }
            if (this.transform.localScale != Vector3.one)
            {
                Debug.LogWarning("The terrain has a non-default scale. This may cause issues.");
            }

            // clear the children
            for (int i = this.transform.childCount - 1; i >= 0; i--)
            {
                GameObject.DestroyImmediate(this.transform.GetChild(i).gameObject);
            }

            // make material
            this._multitextureMat = new Material(Shader.Find("Custom/MultiTexture"));
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
            _multitextureMat.SetTexture($"_TextureArray", textureArray);

            // seed
            if (_worldSeedString == "")
            {
                _worldSeed = ((int)DateTime.Now.Ticks);
            }
            else
            {
                _worldSeed = Helpers.MultiHash(_worldSeedString);
            }
            for (int i = 0; i < _biomes.Count; i++)
            {
                _biomes[i].GetHeightmap().SetSeed(_worldSeed);
            }

            // chunk
            int chunkCount = 3;
            _chunks = new GameObject[chunkCount, chunkCount];
            for (int x = 0; x < chunkCount; x++)
            {
                for (int z = 0; z < chunkCount; z++)
                {
                    _chunks[x, z] = GenerateChunk(x, z);
                }
            }

            StitchChunks();
        }

        private void StitchChunks()
        {
            for (int x = 0; x < _chunks.GetLength(0); x++)
            {
                for (int z = 0; z < _chunks.GetLength(1); z++)
                {
                    // stitch along z
                    if (z > 0)
                    {
                        Mesh meshRight = _chunks[x, z].GetComponent<MeshFilter>().sharedMesh; // this chunk
                        Mesh meshLeft = _chunks[x, z - 1].GetComponent<MeshFilter>().sharedMesh; // the chunk in the (0, -1) direction
                        Vector3[] verticesLeft = meshLeft.vertices;
                        Vector3[] verticesRight = meshRight.vertices;

                        for (int i = 0; i < _chunkResolution; i++)
                        {
                            int idxLeft = i * _chunkResolution;
                            int idxRight = i * _chunkResolution + _chunkResolution - 1;
                            float avg = verticesLeft[idxRight].y + verticesRight[idxLeft].y;
                            avg /= 2f;
                            verticesLeft[idxRight].y = avg;
                            verticesRight[idxLeft].y = avg;
                        }

                        meshLeft.vertices = verticesLeft;
                        meshRight.vertices = verticesRight;
                    }
                    // stitch along x
                    if (x > 0)
                    {
                        Mesh meshTop = _chunks[x, z].GetComponent<MeshFilter>().sharedMesh; // this chunk
                        Mesh meshBottom = _chunks[x - 1, z].GetComponent<MeshFilter>().sharedMesh; // the chunk in the (-1, 0) direction
                        Vector3[] verticesTop = meshTop.vertices;
                        Vector3[] verticesBottom = meshBottom.vertices;

                        for (int i = 0; i < _chunkResolution; i++)
                        {
                            int idxTop = i;
                            int idxBottom = (_chunkResolution - 1) * _chunkResolution + i;
                            float avg = verticesTop[idxTop].y + verticesBottom[idxBottom].y;
                            avg /= 2f;
                            verticesTop[idxTop].y = avg;
                            verticesBottom[idxBottom].y = avg;
                        }

                        meshTop.vertices = verticesTop;
                        meshBottom.vertices = verticesBottom;
                    }
                }
            }
        }

        public GameObject GenerateChunk(int chunkX, int chunkZ)
        {
            Mesh mesh = new Mesh();
            mesh.name = $"Chunk Mesh ({chunkX}, {chunkZ})";

            BiomeMap biomeMap = new(
                worldSeed: _worldSeed,
                biomeCount: _biomes.Count,
                chunkSize: _chunkSize,
                chunkX: chunkX,
                chunkZ: chunkZ,
                biomesPerChunk: _biomesPerChunk
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

                    float worldX = (chunkX * _chunkSize) + offsetX;
                    float worldZ = (chunkZ * _chunkSize) + offsetZ;

                    // calculate a weighted height based on the biomes
                    BiomeWeight[] biomeWeights = biomeMap.Sample(offsetX, offsetZ);
                    float height = 0;
                    // we will also find which biome has the highest weight
                    int primaryBiomeIdx = -1;
                    float heighestWeight = -1;
                    foreach (BiomeWeight biomeWeight in biomeWeights)
                    {
                        height += _biomes[biomeWeight.BiomeIndex].GetHeightmap().GetHeight(worldX, worldZ) * biomeWeight.Weight;
                        if (biomeWeight.Weight > heighestWeight)
                        {
                            heighestWeight = biomeWeight.Weight;
                            primaryBiomeIdx = biomeWeight.BiomeIndex;
                        }
                    }
                    vertices[i] = new Vector3(offsetX, height, offsetZ);

                    // put the texture index in the uv2.x
                    int textureIdx = primaryBiomeIdx;
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
            chunk.GetComponent<MeshRenderer>().sharedMaterial = _multitextureMat;
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
                int biomeIndex = (int)uv2s[i].x;
                Biome biome = _biomes[biomeIndex];
                for (int j = 0; j < biome.GetFeatures().Count; j++)
                {
                    BiomeFeature feature = biome.GetFeatures()[j];
                    double randomProbability = rand.NextDouble();
                    // double featureProbability = feature.Frequency / 10000.0f;
                    // double featureProbability = feature.Frequency * Math.Log(feature.Frequency + 1.0f) / 10000.0f;
                    double featureProbability = 0.1f * Convert.ToInt32(feature.Frequency != 0) * (1f / (1f + Mathf.Exp(-((feature.Frequency * 0.75f - 50f)) / 5f)));
                    if (randomProbability < featureProbability)
                    {
                        if (feature.Prefab != null)
                        {
                            GameObject spawnedObject = Instantiate(feature.Prefab);
                            spawnedObject.transform.localScale = feature.Scale;
                            // move to chunk position
                            spawnedObject.transform.position = new Vector3(chunkX, 0, chunkZ) * _chunkSize;
                            // set position within chunk
                            spawnedObject.transform.position += vertex;
                            if (feature.SetNormal)
                            {
                                // make normal to terrain
                                Vector3 normal = mesh.normals[i];
                                spawnedObject.transform.rotation = Quaternion.FromToRotation(Vector3.up, normal);
                            }
                            // spawnedObject.transform.rotation = Quaternion.Euler(0, rand.Next(0, 360), 0);
                            spawnedObject.transform.Rotate(Vector3.up, rand.Next(0, 360));
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

            return chunk;
        }
    }
}