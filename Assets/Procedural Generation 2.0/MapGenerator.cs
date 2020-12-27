using UnityEngine;

namespace Procedural_Generation_2._0
{
    [RequireComponent(typeof(MapDisplay))]
    public class MapGenerator : MonoBehaviour
    {
        public int mapWidth = 100;
        public int mapHeight = 100;
        public float noiseScale = .3f;

        public bool autoUpdate = true;

        public void GenerateMap()
        {
            var noiseMap = Noise.GenerateNoiseMap(mapWidth, mapHeight, noiseScale); // Generate noise map.

            // Visualize noise map.
            var display = GetComponent<MapDisplay>();
            display.DrawNoiseMap(noiseMap);
        }
    }
}
