using System;
using UnityEngine;
using Random = System.Random;

namespace Procedural_Generation_2._0
{
    [RequireComponent(typeof(MapDisplay))]
    public class MapGenerator : MonoBehaviour
    {
        public enum DrawMode
        {
            NoiseMap,
            ColorMap,
            Mesh
        }

        public const int MapChunkSize = 241;

        public DrawMode drawMode;

        [Range(0, 6)] public int levelOfDetail;

        public float noiseScale = 25f;

        [Range(1, 30)] public int octaves = 4;

        [Range(0, 1)] public float persistence = 0.5f;

        public float lacunarity = 2f;
        public int seed;
        public Vector2 offset;

        public float meshHeightMultiplier = 20;
        public AnimationCurve meshHeightCurve;

        public bool autoUpdate = true;

        public bool randomSeed;

        public TerrainType[] regions;

        private void OnValidate()
        {
            if (lacunarity < 1) lacunarity = 1; // Set minimum lacunarity to 1.
        }

        public void GenerateMap()
        {
            if (randomSeed) seed = new Random().Next(int.MinValue, int.MaxValue); // Set random seed if wanted.
            var noiseMap = Noise.GenerateNoiseMap(MapChunkSize, MapChunkSize, noiseScale, seed, octaves, persistence, lacunarity, offset); // Generate noise map.

            var colorMap = new Color[MapChunkSize * MapChunkSize];

            // Loop through every coordinate and get the current height value. Then we find our region which has its height value in range.
            // After that we assign the corresponding color to a 2d array of colors at the same coordinates we got the height from.
            for (var y = 0; y < MapChunkSize; y++) // Loop y
            for (var x = 0; x < MapChunkSize; x++) // Loop x
            {
                var currentHeight = noiseMap[x, y]; // Get height

                for (var i = 0; i < regions.Length; i++) // Find region
                {
                    if (!(currentHeight <= regions[i].height)) continue; // Find region
                    colorMap[y * MapChunkSize + x] = regions[i].color; // Assign color
                    break;
                }
            }

            // Visualize noise map.
            var display = GetComponent<MapDisplay>();
            switch (drawMode)
            {
                case DrawMode.NoiseMap:
                    display.DrawTexture(TextureGenerator.TextureFromHeightMap(noiseMap));
                    break;
                case DrawMode.ColorMap:
                    display.DrawTexture(TextureGenerator.TextureFromColorMap(colorMap, MapChunkSize, MapChunkSize));
                    break;
                case DrawMode.Mesh:
                    display.DrawMesh(MeshGenerator.GenerateTerrainMesh(noiseMap, meshHeightMultiplier, meshHeightCurve, levelOfDetail), TextureGenerator.TextureFromColorMap(colorMap, MapChunkSize, MapChunkSize));
                    break;
            }
        }
    }
}

[Serializable]
public struct TerrainType
{
    public string name;
    public float height;
    public Color color;
}
