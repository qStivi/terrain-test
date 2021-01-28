using System;
using UnityEngine;
using Random = System.Random;

public static class Noise
{
    public enum NormalizeMode
    {
        Local,
        Global
    }

    // This method creates a two dimensional float array and assigns pseudo random values to it using perlin noise.
    public static float[,] GenerateNoiseMap(int mapWidth, int mapHeight, NoiseSettings settings, Vector2 sampleCentre)
    {
        if (settings.randomSeed) settings.seed = new Random().Next(int.MinValue, int.MaxValue); // randomize seed if desired.

        // This is our "main variable". A 2d array of height values which can be represented as a map.
        // For each pair of values (or coordinates) a height value will be calculated using perlin noise, random offsets, octaves, a seed, a persistence value and a lacunarity value.
        var noiseMap = new float[mapWidth, mapHeight];

        // Create a pseudo random value.
        var prng = new Random(settings.seed); // Create new instance of random generator with seed so it works like minecraft seeds.
        var octaveOffset = new Vector2[settings.octaves]; // Octaves are stacked layers of noise with different resolutions

        var maxPossibleHeight = 0f; // This will be changed every octave using the amplitude value.
        var amplitude = 1f; // This will be changed every octave using the persistence value.

        // calculating octaves
        for (var i = 0; i < settings.octaves; i++)
        {
            // Larger ranges tend to produce repeating values. 
            var offsetX = prng.Next(-100000, 100000) + settings.offset.x + sampleCentre.x;
            var offsetY = prng.Next(-100000, 100000) - settings.offset.y - sampleCentre.y;
            octaveOffset[i] = new Vector2(offsetX, offsetY);

            maxPossibleHeight += amplitude; // Gets larger each octave.
            amplitude *= settings.persistence; // Gets smaller each octave.
        }

        // Using this no normalize noise map after generating.
        var maxLocalNoiseHeight = float.MinValue;
        var minLocalNoiseHeight = float.MaxValue;

        // Using this to "zoom" to the center instead of top right.
        var halfWidth = mapWidth / 2f;
        var halfHeight = mapHeight / 2f;

        // calculating actual noise
        for (var y = 0; y < mapHeight; y++)
        for (var x = 0; x < mapWidth; x++)
        {
            amplitude = 1f; // This will be changed every octave using the persistence value.
            var frequency = 1f; // This will be changed every octave using the lacunarity value.
            var noiseHeight = 0f; // Current height value

            for (var i = 0; i < settings.octaves; i++)
            {
                // apply scale factor, frequency and random offset.
                var sampleX = (x - halfWidth + octaveOffset[i].x) / settings.scale * frequency;
                var sampleY = (y - halfHeight + octaveOffset[i].y) / settings.scale * frequency;
                
                // Calculate perlinValue for our current "pixel". Then change range to -1 to 1 for more interesting values.
                // Mathf.PerlinNoise() returns a value between 0 and 1 by default.
                var perlinValue = Mathf.PerlinNoise(sampleX, sampleY) * 2 - 1;

                // calculate noiseHeight.
                noiseHeight += perlinValue * amplitude; // Apply amplitude.

                amplitude *= settings.persistence; // Gets smaller each octave.
                frequency *= settings.lacunarity; // Gets larger each octave.
            }

            // Set min and max noiseHeight.
            if (noiseHeight > maxLocalNoiseHeight) maxLocalNoiseHeight = noiseHeight;
            if (noiseHeight < minLocalNoiseHeight) minLocalNoiseHeight = noiseHeight;

            noiseMap[x, y] = noiseHeight; // Save calculated height in our map

            // normalize calculated value according to normalize mode.
            if (settings.normalizeMode != NormalizeMode.Global) continue;
            var normalizedHeight = (noiseMap[x, y] + 1) / maxPossibleHeight / 0.9f;
            noiseMap[x, y] = Mathf.Clamp(normalizedHeight, 0, int.MaxValue);
        }

        // normalize calculated value according to normalize mode.
        // ReSharper disable once InvertIf
        if (settings.normalizeMode == NormalizeMode.Local)
            for (var y = 0; y < mapHeight; y++)
            for (var x = 0; x < mapWidth; x++)
                noiseMap[x, y] = Mathf.InverseLerp(minLocalNoiseHeight, maxLocalNoiseHeight, noiseMap[x, y]);

        return noiseMap;
    }
}

[Serializable]
public class NoiseSettings
{
    public Noise.NormalizeMode normalizeMode;

    public float scale = 25f;

    [Range(1, 30)] public int octaves = 4;
    [Range(0, 1)] public float persistence = 0.5f;
    [Min(1)] public float lacunarity = 2f;

    public int seed;
    public Vector2 offset;

    public bool randomSeed;

    public void ValidateValues()
    {
        scale = Mathf.Max(scale, 0.01f);
        octaves = Mathf.Max(octaves, 1);
        lacunarity = Mathf.Max(lacunarity, 1);
        persistence = Mathf.Clamp01(persistence);
    }
}
