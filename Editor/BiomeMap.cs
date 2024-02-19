using System;
using System.Collections.Generic;
/// <summary>
/// The BiomeMap is a voronoi diagram in which each cell is a biome. It extends infinitely in all directions.
/// Obviously we can't generate it all beforehand, so I generate a local region on sample.
/// I place voronoi seeds on a grid and jitter them.
/// </summary>
public class BiomeMap
{
    private int _worldSeed;
    private int _biomeCount;
    private float _chunkSize;

    private List<VoronoiSeed> _voronoiSeeds;

    public BiomeMap(int worldSeed, int biomeCount, float chunkSize, int chunkX, int chunkZ)
    {
        _worldSeed = worldSeed;
        _biomeCount = biomeCount;
        _chunkSize = chunkSize;

        _voronoiSeeds = new List<VoronoiSeed>();

        // generate voronoi seeds for the chunk and its neighbors
        for(int x = -1; x <= 1; x++)
        {
            for(int z = -1; z <= 1; z++)
            {
                int hash = Helpers.MultiHash(_worldSeed, chunkX + x, chunkZ + z);
                System.Random random = new System.Random(hash);
               
                for(int i = 0; i < 3; i++)
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
    public BiomeWeight[] Sample(float worldX, float worldY)
    {
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
            float distance = (worldX - _voronoiSeeds[i].X) * (worldX - _voronoiSeeds[i].X) +
                         (worldY - _voronoiSeeds[i].Y) * (worldY - _voronoiSeeds[i].Y);

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