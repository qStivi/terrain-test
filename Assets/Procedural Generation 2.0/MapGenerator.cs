using System;
using UnityEngine;

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

        public DrawMode drawMode;

        private const int MapChunkSize = 241;
        [Range(0, 6)]
        public int levelOfDetail;
        public float noiseScale = 25f;

        [Range(1, 30)] public int octaves = 4;

        [Range(0, 1)] public float persistence = 0.5f;

        public float lacunarity = 2f;
        public int seed;
        public Vector2 offset;

        public float meshHeightMultiplier = 20;
        public AnimationCurve meshHeightCurve;

        public bool autoUpdate = true;

        public TerrainType[] regions;

        private void OnValidate()
        {
            if (lacunarity < 1) lacunarity = 1; // Set minimum lacunarity to 1.
        }

        public void GenerateMap()
        {
            var noiseMap = Noise.GenerateNoiseMap(MapChunkSize, MapChunkSize, noiseScale, seed, octaves, persistence, lacunarity, offset); // Generate noise map.

            var colorMap = new Color[MapChunkSize * MapChunkSize];
            for (var y = 0; y < MapChunkSize; y++)
            for (var x = 0; x < MapChunkSize; x++)
            {
                var currentHeight = noiseMap[x, y];

                for (var i = 0; i < regions.Length; i++)
                {
                    if (!(currentHeight <= regions[i].height)) continue;
                    colorMap[y * MapChunkSize + x] = regions[i].color;
                    break;
                }
            }

            // Visualize noise map.
            var display = GetComponent<MapDisplay>();
            if (drawMode == DrawMode.NoiseMap)
                display.DrawTexture(TextureGenerator.TextureFromHeightMap(noiseMap));
            else if (drawMode == DrawMode.ColorMap)
                display.DrawTexture(TextureGenerator.TextureFromColorMap(colorMap, MapChunkSize, MapChunkSize));
            else if (drawMode == DrawMode.Mesh) display.DrawMesh(MeshGenerator.GenerateTerrainMesh(noiseMap, meshHeightMultiplier, meshHeightCurve, levelOfDetail), TextureGenerator.TextureFromColorMap(colorMap, MapChunkSize, MapChunkSize));
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
