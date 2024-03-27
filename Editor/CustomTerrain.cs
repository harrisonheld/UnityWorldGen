using System;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;
using System.IO;
using System.Linq;

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
        [Header("Debug Settings")]
        [Tooltip("If true, a box will be drawn around each chunk. Make sure Gizmos are enabled in the Unity Editor.")]
        [SerializeField] private bool _drawChunkGizmos = true;

        [Header("Generation Settings")]
        [Tooltip("The seed string used to generate the terrain. If left empty, a random seed will be used.")]
        [SerializeField] private string _worldSeedString = "";
        [Tooltip("The seed string used to generate the features. If left empty, a random seed will be used.")]
        [SerializeField] private string _featureSeedString = "";

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

        [SerializeField]
        private GameObject[,] _chunks;
        private int _chunkCount = 3;
        private int _featureSeed;


        private Dictionary<(int, int), (Vector3[], Vector2[])> _chunkInfo = new Dictionary<(int chunkX, int chunkZ), (Vector3[] vertices, Vector2[] uv2s)>();

        public void Awake()
        {
            // generate the terrain at runtime
            GenerateTerrain();
        }


        public Biome GetBiome(string biomeId) 
        {
            foreach (var biome in _biomes)
            {
                if (biome.GetBiomeId() == biomeId)
                {
                    return biome;
                }
            }

            Debug.LogWarning($"Biome with ID {biomeId} not found.");
            return null;
        }

        public void AddBiome(Biome newBiome)
        {
            this._biomes.Add(newBiome);
        }
        public void DeleteBiome(string biomeId)
        {
            for (int i = 0; i < _biomes.Count; i++)
            {
                if (_biomes[i].GetBiomeId() == biomeId)
                {
                    Debug.Log(_biomes[i].GetBiomeId());
                    _biomes.RemoveAt(i);
                    Debug.Log($"Biome with ID {biomeId} deleted.");
                    return; // Exit the method after deleting the biome
                }
            }
            Debug.LogWarning($"Biome with ID {biomeId} not found.");
        }
        public void GenerateTerrain()
        {
            // errors
            if (_biomes.Count == 0)
            {
                Debug.LogError("Cannot generate terrain because no biomes have been added to the terrain.");
                return;
            }
            // ensure all biomes have heightmap
            for (int i = 0; i < _biomes.Count; i++)
            {
                if(_biomes[i].GetHeightmap() == null) {
                    Debug.LogError($"Biome {i} does not have a heightmap assigned.");
                    return;
                }
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
            _chunks = new GameObject[_chunkCount, _chunkCount];
            for (int x = 0; x < _chunkCount; x++)
            {
                for (int z = 0; z < _chunkCount; z++)
                {
                    _chunks[x, z] = GenerateChunk(x, z);
                    (Vector3[], Vector2[]) chunk_info = _chunkInfo[(x, z)];
                    GenerateChunkFeatures(x, z, chunk_info.Item1, chunk_info.Item2);
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
        private GameObject GenerateChunk(int chunkX, int chunkZ)
        {
            Mesh mesh = new Mesh();
            mesh.name = $"Chunk Mesh ({chunkX}, {chunkZ})";

            BiomeMap biomeMap = new(
                worldSeed: _worldSeed,
                biomes: _biomes,
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

            _chunkInfo[(chunkX, chunkZ)] = (vertices, uv2s);
            // _chunkInfo[chunkX, chunkZ] = (vertices, uv2s);

            return chunk;
        }

        public void GenerateAllFeatures()
        {

            // Iterate over each chunk and remove all features
            for (int i = this.transform.childCount - 1; i >= 0; i--)
            {
                Transform child = this.transform.GetChild(i);

                for (int j = child.gameObject.transform.childCount - 1; j >= 0; j--)
                {
                    Transform grandchild = child.gameObject.transform.GetChild(j);

                    // Destroy the child GameObject
                    GameObject.DestroyImmediate(grandchild.gameObject);
                }
            }
            // generate features for each chunk
            for (int x = 0; x < _chunkCount; x++)
            {
                for (int z = 0; z < _chunkCount; z++)
                {
                    (Vector3[], Vector2[]) chunk_info = _chunkInfo[(x, z)];
                    GenerateChunkFeatures(x, z, chunk_info.Item1, chunk_info.Item2);
                }
            }
        }
        public void GenerateChunkFeatures(int chunkX, int chunkZ, Vector3[] vertices, Vector2[] uv2s)
        {
            // seed
            if (_featureSeedString == "")
            {
                _featureSeed = ((int)DateTime.Now.Ticks);
            }
            else
            {
                _featureSeed = Helpers.MultiHash(_featureSeedString);
            }

            int chunkSeed = Helpers.MultiHash(_featureSeed, chunkX, chunkZ);
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

                    double featureProbability = Math.Pow(250.0f / _chunkResolution, 2) * 0.1f * Convert.ToInt32(feature.Frequency != 0) * (1f / (1f + Mathf.Exp(-((feature.Frequency * 0.75f - 50f)) / 5f)));
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
                                Mesh mesh = _chunks[chunkX, chunkZ].GetComponent<MeshFilter>().sharedMesh;
                                Vector3 normal = mesh.normals[i];
                                spawnedObject.transform.rotation = Quaternion.FromToRotation(Vector3.up, normal);
                            }
                            spawnedObject.transform.Rotate(Vector3.up, rand.Next(0, 360));
                            // set parent as chunk
                            GameObject chunk = _chunks[chunkX, chunkZ];
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

        public void ExportChunks(string path)
        {
            if (_chunks == null)
            {
                Debug.LogError("Cannot export chunks because the terrain has not been generated yet. Try regenerating.");
                return;
            }
            if (_chunks.Length == 0)
            {
                Debug.LogError("Cannot export chunks because there are no chunks to export. Try regenerating.");
                return;
            }

            GameObject sampleChunk = _chunks[0, 0];
            int chunkverts = sampleChunk.GetComponent<MeshFilter>().sharedMesh.vertices.Length;
            int limit = 50;
            if(chunkverts > limit*limit) { 
                Debug.LogWarning($"The chunk resolution is too high for exporting. Please reduce the chunk resolution to {limit} or lower and regenerating.");
                return;
            }

            // Ensure path is a directory path
            if (Path.HasExtension(path))
            {
                Debug.LogError("Path is not a directory.");
                return;
            }

            // Clear the directory if it exists
            if (Directory.Exists(path))
                Directory.Delete(path, true);
            Directory.CreateDirectory(path);

            // Export each chunk as a separate OBJ file
            try
            {
                MeshExporter exporter = new MeshExporter();
                for (int x = 0; x < _chunkCount; x++)
                {
                    for (int z = 0; z < _chunkCount; z++)
                    {
                        GameObject chunk = _chunks[x, z];
                        Mesh mesh = chunk.GetComponent<MeshFilter>().sharedMesh;
                        string chunkPath = Path.Combine(path, $"chunk-{x}-{z}.obj");
                        exporter.ExportMesh(mesh, chunkPath);
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Error exporting chunks: {e.Message}");
                return;
            }

            Debug.Log($"Exported all chunks to '{path}'.");
        }



        private void OnDrawGizmos()
        {
            // draw chunk border gizmos
            if(_drawChunkGizmos && _chunks != null)
            {
                Gizmos.color = Color.red;

                for (int x = 0; x < _chunks.GetLength(0); x++)
                {
                    for (int z = 0; z < _chunks.GetLength(1); z++)
                    {
                        Vector3 chunkCenter = new Vector3(x * _chunkSize, 0, z * _chunkSize);
                        Vector3 chunkSize = new Vector3(_chunkSize, 0, _chunkSize);
                        Gizmos.DrawWireCube(chunkCenter, chunkSize);
                    }
                }
            }
        }


        public void SetPlayerPos(Vector3 pos)
        {
            // check error
            if (_chunks == null)
            {
                Debug.LogError("Cannot set player position because the terrain has not been generated yet.");
                return;
            }
            // get chunk the player is in, remember that chunk 0,0 is at the origin
            int chunkX = Mathf.FloorToInt((pos.x / _chunkSize) + 0.5f);
            int chunkZ = Mathf.FloorToInt((pos.z / _chunkSize) + 0.5f);
            if(chunkX < 0 || chunkX >= _chunks.GetLength(0) || chunkZ < 0 || chunkZ >= _chunks.GetLength(1))
            {
                return;
            }
            GameObject chunk = _chunks[chunkX, chunkZ];

            // get player offset within chunk
            Vector3 offset = pos - chunk.transform.position;
            offset += new Vector3(_chunkSize / 2, 0, _chunkSize / 2);
        
            // get the dominant biome at that offset
            Mesh mesh = chunk.GetComponent<MeshFilter>().sharedMesh;
            int x = Mathf.FloorToInt(offset.x / _chunkSize * _chunkResolution);
            int z = Mathf.FloorToInt(offset.z / _chunkSize * _chunkResolution);
            int i = x * _chunkResolution + z;
            int biomeIndex = (int)mesh.uv2[i].x; // uv2.x is the texture index
            Biome biome = _biomes[biomeIndex];

            // set skybox
            RenderSettings.skybox = biome.GetSkybox();
        }
    }
}