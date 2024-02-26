using System;
using System.Collections.Generic;

/// <summary>
/// A BiomeMap is a Voronoi diagram that assigns a biome to each cell.
/// To use this class, generate a BiomeMap for each chunk in the world, and sample it at an offset within the chunk to determine the
/// weights for each biome at that position.
/// </summary>
public class BiomeMap
{
    private int _worldSeed;
    private int _biomeCount;
    private int _biomesPerChunk;
    private float _chunkSize;

    private List<VoronoiSeed> _voronoiSeeds;

    public BiomeMap(int worldSeed, int biomeCount, float chunkSize, int chunkX, int chunkZ, int biomesPerChunk)
    {
        _worldSeed = worldSeed;
        _biomeCount = biomeCount;
        _chunkSize = chunkSize;
        _biomesPerChunk = biomesPerChunk;

        _voronoiSeeds = new List<VoronoiSeed>();

        // generate voronoi seeds for the chunk and its neighbors
        for(int x = -1; x <= 1; x++)
        {
            for(int z = -1; z <= 1; z++)
            {
                int hash = Helpers.MultiHash(_worldSeed, chunkX + x, chunkZ + z);
                System.Random random = new System.Random(hash);
               
                for(int i = 0; i < _biomesPerChunk; i++)
                {
                    float startX = x * _chunkSize;
                    float startZ = z * _chunkSize;
                    float seedX = startX + (float)random.NextDouble() * _chunkSize;
                    float seedZ = startZ + (float)random.NextDouble() * _chunkSize;
                    int biome = random.Next(0, _biomeCount);

                    VoronoiSeed voronoiSeed = new VoronoiSeed
                    {
                        X = seedX,
                        Y = seedZ,
                        Biome = biome
                    };
                    _voronoiSeeds.Add(voronoiSeed);
                }
            }
        }
    }

    /// <summary>
    /// given an offset within the chunk, return an array of BiomeWeights for the position, one for each biome in the map.
    /// </summary>
    public BiomeWeight[] Sample(float chunkOffsetX, float chunkOffsetZ)
    {
        // check if sample pos is in the chunk
        if(   chunkOffsetX < - _chunkSize / 2f
           || chunkOffsetX > _chunkSize / 2f
           || chunkOffsetZ < - _chunkSize / 2f
           || chunkOffsetZ > _chunkSize / 2f)
        {
            throw new ArgumentException("Sample position is not within the chunk");
        }

        // generate blank biome weights
        BiomeWeight[] weights = new BiomeWeight[_biomeCount];
        for(int i = 0; i < _biomeCount; i++)
        {
            weights[i] = new BiomeWeight();
            weights[i].BiomeIndex = i;
            weights[i].Weight = 0;
        }

        // add weight contribution of each voronoi seed
        float totalWeight = 0f;
        for(int i = 0; i < _voronoiSeeds.Count; i++)
        {
            int biome = _voronoiSeeds[i].Biome;
            float distance = (chunkOffsetX - _voronoiSeeds[i].X) * (chunkOffsetX - _voronoiSeeds[i].X) +
                         (chunkOffsetZ - _voronoiSeeds[i].Y) * (chunkOffsetZ - _voronoiSeeds[i].Y);

            // apply a falloff function to the distance
            float weight = 1.0f / (1.0f + distance * distance * distance);
            weights[biome].Weight += weight;
            totalWeight += weight;
        }

        // Normalize weights to ensure they sum to 1
        for (int i = 0; i < weights.Length; i++)
        {
            weights[i].Weight /= totalWeight;
        }

        return weights;
    }

    private struct VoronoiSeed
    {
        public float X;
        public float Y;
        public int Biome;
    }
}
public struct BiomeWeight
{
    public int BiomeIndex;
    public float Weight;
}