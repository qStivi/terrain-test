using UnityEngine;

namespace Procedural_Generation_2._0
{
    [RequireComponent(typeof(MapDisplay))]
    public class MapGenerator : MonoBehaviour
    {
        public int mapWidth = 100;
        public int mapHeight = 100;
        public float noiseScale = 25f;

        [Range(1, 30)] public int octaves = 4;

        [Range(0, 1)] public float persistence = 0.5f;

        public float lacunarity = 2f;
        public int seed;
        public Vector2 offset;

        public bool autoUpdate = true;

        private void OnValidate()
        {
            if (mapWidth < 1) mapWidth = 1;

            if (mapHeight < 1) mapHeight = 1;

            if (lacunarity < 1) lacunarity = 1;
        }

        public void GenerateMap()
        {
            var noiseMap = Noise.GenerateNoiseMap(mapWidth, mapHeight, noiseScale, seed, octaves, persistence, lacunarity, offset); // Generate noise map.

            // Visualize noise map.
            var display = GetComponent<MapDisplay>();
            display.DrawNoiseMap(noiseMap);
        }
    }
}