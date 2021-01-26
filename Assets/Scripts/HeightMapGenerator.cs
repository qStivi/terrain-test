using System.Collections;
using System.Collections.Generic;
using Data;
using UnityEngine;

public static class HeightMapGenerator
{
    public static HeightMap GenerateHeightMap(int width, int height, HeightMapSettings settings, Vector2 sampleCentre)
    {
        var values = Noise.GenerateNoiseMap(width, height, settings.noiseSettings, sampleCentre);
        var heightCurve_threadsave = new AnimationCurve(settings.heightCurve.keys);

        var minValue = float.MaxValue;
        var maxValue = float.MinValue;

        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                values[i, j] *= heightCurve_threadsave.Evaluate(values[i, j]) * settings.heightMultiplier;

                if (values[i, j] > maxValue)
                {
                    maxValue = values[i, j];
                }
                if (values[i, j] < minValue)
                {
                    minValue = values[i, j];
                }
            }
        }

        return new HeightMap(values, minValue, maxValue);
    }
}
public struct HeightMap
{
    public HeightMap(float[,] values, float minValue, float maxValue)
    {
        this.values = values;
        this.minValue = minValue;
        this.maxValue = maxValue;
    }

    public readonly float[,] values;
    public readonly float minValue;
    public readonly float maxValue;
}
