using Data;
using UnityEngine;

public static class HeightMapGenerator
{
    public static HeightMap GenerateHeightMap(int width, int height, HeightMapSettings settings, Vector2 sampleCentre)
    {
        var values = Noise.GenerateNoiseMap(width, height, settings.noiseSettings, sampleCentre);
        var heightCurveThreadSave = new AnimationCurve(settings.heightCurve.keys);

        var minValue = float.MaxValue;
        var maxValue = float.MinValue;

        for (var i = 0; i < width; i++)
        for (var j = 0; j < height; j++)
        {
            values[i, j] *= heightCurveThreadSave.Evaluate(values[i, j]) * settings.heightMultiplier;

            if (values[i, j] > maxValue) maxValue = values[i, j];
            if (values[i, j] < minValue) minValue = values[i, j];
        }

        return new HeightMap(values, minValue, maxValue);
    }
}

public readonly struct HeightMap
{
    public HeightMap(float[,] values, float minValue, float maxValue)
    {
        Values = values;
        MINValue = minValue;
        MAXValue = maxValue;
    }

    public readonly float[,] Values;
    public readonly float MINValue;
    public readonly float MAXValue;
}
