using UnityEngine;

namespace Procedural_Generation_2._0
{
    public static class Noise
    {
        // This method creates a two dimensional float array and assigns pseudo random values to it using perlin noise.
        public static float[,] GenerateNoiseMap(int mapWidth, int mapHeight, float scale)
        {
            var noiseMap = new float[mapWidth, mapHeight];

            if (scale == 0) scale = 0.00001f; // prevent division by 0.

            for (var y = 0; y < mapHeight; y++)
            for (var x = 0; x < mapWidth; x++)
            {
                // apply scale factor.
                var sampleX = x / scale;
                var sampleY = y / scale;

                // calculate and store perlin value.
                var perlinValue = Mathf.PerlinNoise(sampleX, sampleY);
                noiseMap[x, y] = perlinValue;
            }

            return noiseMap;
        }
    }
}