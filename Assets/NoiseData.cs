using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class NoiseData : UpdatableData
{
    public Noise.NormalizeMode normalizeMode;
    
    public float noiseScale = 25f;

    [Range(1, 30)] public int octaves = 4;
    [Range(0, 1)] public float persistence = 0.5f;
    [Min(1)] public float lacunarity = 2f;

    public int seed;
    public Vector2 offset;

}
