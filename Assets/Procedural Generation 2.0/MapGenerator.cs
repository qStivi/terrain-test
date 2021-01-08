using System;
using System.Collections.Generic;
using System.Threading;
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
        [Min(1)] public float lacunarity = 2f;

        public int seed;
        public Vector2 offset;

        public float meshHeightMultiplier = 20;
        public AnimationCurve meshHeightCurve;


        public bool autoUpdate = true;
        public TerrainType[] regions;

        public bool randomSeed;

        private readonly Queue<MapThreadInfo<MapData>> mapDataThreadInfoQueue = new Queue<MapThreadInfo<MapData>>();
        private readonly Queue<MapThreadInfo<MeshData>> meshDataThredInfoQueue = new Queue<MapThreadInfo<MeshData>>();

        private void Update()
        {
            if (mapDataThreadInfoQueue.Count > 0)
                for (var i = 0; i < mapDataThreadInfoQueue.Count; i++)
                {
                    var threadInfo = mapDataThreadInfoQueue.Dequeue();
                    threadInfo.callback(threadInfo.parameter);
                }

            if (meshDataThredInfoQueue.Count > 0)
                for (var i = 0; i < meshDataThredInfoQueue.Count; i++)
                {
                    var threadInfo = meshDataThredInfoQueue.Dequeue();
                    threadInfo.callback(threadInfo.parameter);
                }
        }

        // Visualize noise map.
        public void DrawMapInEditor()
        {
            var mapData = GenerateMapData();
            var display = GetComponent<MapDisplay>();
            switch (drawMode)
            {
                case DrawMode.NoiseMap:
                    display.DrawTexture(TextureGenerator.TextureFromHeightMap(mapData.heightMap));
                    break;
                case DrawMode.ColorMap:
                    display.DrawTexture(TextureGenerator.TextureFromColorMap(mapData.colorMap, MapChunkSize, MapChunkSize));
                    break;
                case DrawMode.Mesh:
                    display.DrawMesh(MeshGenerator.GenerateTerrainMesh(mapData.heightMap, meshHeightMultiplier, meshHeightCurve, levelOfDetail), TextureGenerator.TextureFromColorMap(mapData.colorMap, MapChunkSize, MapChunkSize));
                    break;
            }
        }

        public void RequestMapData(Action<MapData> callback)
        {
            void ThreadStart()
            {
                MapDataThread(callback);
            }

            new Thread(ThreadStart).Start();
        }

        private void MapDataThread(Action<MapData> callback)
        {
            var mapData = GenerateMapData();
            lock (mapDataThreadInfoQueue)
            {
                mapDataThreadInfoQueue.Enqueue(new MapThreadInfo<MapData>(callback, mapData));
            }
        }

        public void RequestMeshData(MapData mapData, Action<MeshData> callback)
        {
            void ThreadStart()
            {
                MeshDataThread(mapData, callback);
            }

            new Thread(ThreadStart).Start();
        }

        private void MeshDataThread(MapData mapData, Action<MeshData> callback)
        {
            var meshData = MeshGenerator.GenerateTerrainMesh(mapData.heightMap, meshHeightMultiplier, meshHeightCurve, levelOfDetail);
            lock (meshDataThredInfoQueue)
            {
                meshDataThredInfoQueue.Enqueue(new MapThreadInfo<MeshData>(callback, meshData));
            }
        }

        private MapData GenerateMapData()
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

            return new MapData(noiseMap, colorMap);
        }

        private struct MapThreadInfo<T>
        {
            public MapThreadInfo(Action<T> callback, T parameter)
            {
                this.callback = callback;
                this.parameter = parameter;
            }

            public readonly Action<T> callback;
            public readonly T parameter;
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

public struct MapData
{
    public MapData(float[,] heightMap, Color[] colorMap)
    {
        this.heightMap = heightMap;
        this.colorMap = colorMap;
    }

    public readonly float[,] heightMap;
    public readonly Color[] colorMap;
}
