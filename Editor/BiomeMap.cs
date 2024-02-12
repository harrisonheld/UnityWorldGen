using System;

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
        int gridX = (int)(worldX / _biomeSize);
        int gridY = (int)(worldY / _biomeSize);

        float minDistance = float.MaxValue;
        int chosenBiome = -1;

        for(int x = gridX - 1; x <= gridX + 1; x++)
        {
            for (int y = gridY - 1; y <= gridY + 1; y++)
            {
                int hash = Helpers.MultiHash(_seed, x, y);
                var random = new System.Random(hash);

                VoronoiSeed point = new VoronoiSeed();
                point.X = (float)x + (float)random.NextDouble() - 0.5f;
                point.Y = (float)y + (float)random.NextDouble() - 0.5f;
                point.Biome = random.Next(_biomeCount);

                float dist = (point.X - worldX) * (point.X - worldX) + (point.Y - worldY) * (point.Y - worldY);
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
