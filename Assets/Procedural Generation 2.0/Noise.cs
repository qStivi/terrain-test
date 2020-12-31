using UnityEngine;
using Random = System.Random;

namespace Procedural_Generation_2._0
{
    public static class Noise
    {
        // This method creates a two dimensional float array and assigns pseudo random values to it using perlin noise.
        public static float[,] GenerateNoiseMap(int mapWidth, int mapHeight, float scale, int seed, int octaves, float persistence, float lacunarity, Vector2 offset)
        {
            var noiseMap = new float[mapWidth, mapHeight];

            // Create a pseudo random values while also using a custom offset.
            // With this we have a random offset which is decided by a seed and a manuel offset.
            var prng = new Random(seed); // Create new instance of random generator with seed so it works like minecraft seeds.
            var octaveOffset = new Vector2[octaves]; // Octaves are multiple stacked layers of noise with different resolutions
            for (var o = 0; o < octaves; o++)
            {
                // Larger ranges tend to produce repeating values. 
                var offsetX = prng.Next(-100000, 100000) + offset.x;
                var offsetY = prng.Next(-100000, 100000) + offset.y;
                octaveOffset[o] = new Vector2(offsetX, offsetY);
            }

            if (scale == 0) scale = 0.00001f; // prevent division by 0.

            // Using this no normalize noise map after generating.
            var maxNoiseHeight = float.MinValue;
            var minNoiseHeight = float.MaxValue;

            // Using this to "zoom" to the center instead of top right.
            var halfWidth = mapWidth / 2f;
            var halfHeight = mapHeight / 2f;

            for (var y = 0; y < mapHeight; y++)
            for (var x = 0; x < mapWidth; x++)
            {
                var amplitude = 1f;
                var frequency = 1f;
                var noiseHeight = 0f; // Current height value

                for (var o = 0; o < octaves; o++)
                {
                    // apply scale factor, frequency and random offset.
                    var sampleX = (x - halfWidth) / scale * frequency + octaveOffset[o].x;
                    var sampleY = (y - halfHeight) / scale * frequency + octaveOffset[o].y;

                    // calculate noiseHeight.
                    var perlinValue = Mathf.PerlinNoise(sampleX, sampleY) * 2 - 1; // Calculate perlinValue for our current "pixel". Then change range to -1 to 1 for more interesting values.
                    // Mathf.PerlinNoise() returns a value between 0 and 1 by default.
                    noiseHeight += perlinValue * amplitude; // Apply amplitude.

                    amplitude *= persistence;
                    frequency *= lacunarity;
                }

                // Set min and max noiseHeight.
                if (noiseHeight > maxNoiseHeight) maxNoiseHeight = noiseHeight;
                else if (noiseHeight < minNoiseHeight) minNoiseHeight = noiseHeight;

                noiseMap[x, y] = noiseHeight;
            }

            // Normalize noise map. (Fit values to range 0 to 1)
            for (var y = 0; y < mapHeight; y++)
            for (var x = 0; x < mapWidth; x++)
                noiseMap[x, y] = Mathf.InverseLerp(minNoiseHeight, maxNoiseHeight, noiseMap[x, y]);

            return noiseMap;
        }
    }
}
