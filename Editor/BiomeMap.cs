using System;
/// <summary>
/// The BiomeMap is a voronoi diagram in which each cell is a biome. It extends infinitely in all directions.
/// Obviously we can't generate it all beforehand, so I generate a local region on sample.
/// I place voronoi seeds on a grid and jitter them.
/// </summary>
public class BiomeMap
{
    private int _seed;
    private int _biomeCount;
    private float _biomeSize;
    public void SetSeed(int seed)
    {
        _seed = seed;
    }
    public void SetBiomeCount(int count)
    {
        _biomeCount = count;
    }
    public void SetBiomeSize(float biomeSize)
    {
        _biomeSize = biomeSize;
    }

    public int Sample(float worldX, float worldY)
    {
        float scaledX = worldX / _biomeSize;
        float scaledY = worldY / _biomeSize;
        int gridX = (int)scaledX;
        int gridY = (int)scaledY;

        float minDistance = float.MaxValue;
        int chosenBiome = -1;

        // generate 4x4 lattice points, so we have 3x3 boxes
        for(int x = gridX - 1; x <= gridX + 2; x++)
        {
            for (int y = gridY - 1; y <= gridY + 2; y++)
            {
                int hash = Helpers.MultiHash(_seed, x, y);
                var random = new System.Random(hash);

                VoronoiSeed point = new VoronoiSeed();
                float jitterX = (float)random.NextDouble() - 0.5f;
                float jitterY = (float)random.NextDouble() - 0.5f;
                point.X = (float)x + jitterX;
                point.Y = (float)y + jitterY;
                point.Biome = random.Next(_biomeCount);

                float dist = (point.X - scaledX) * (point.X - scaledX) + (point.Y - scaledY) * (point.Y - scaledY);
                if (dist < minDistance)
                {
                    minDistance = dist;
                    chosenBiome = point.Biome;
                }
            }
        }

        return chosenBiome;
    }

    private struct VoronoiSeed
    {
        public float X;
        public float Y;
        public int Biome;
    }
}
