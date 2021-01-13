using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using Random = System.Random;

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

        public Noise.NormalizeMode normalizeMode;

        public const int mapChunkSize = 241;
        [Range(0, 6)] public int editorPreviewLOD;
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

        Queue<MapThreadInfo<MapData>> mapDataThreadInfoQueue = new Queue<MapThreadInfo<MapData>>();
        Queue<MapThreadInfo<MeshData>> meshDataThredInfoQueue = new Queue<MapThreadInfo<MeshData>>();


        // Visualize noise map.
        public void DrawMapInEditor()
        {
            var mapData = GenerateMapData(Vector2.zero);
            
            var display = FindObjectOfType<MapDisplay>();
            if (drawMode == DrawMode.NoiseMap)
                display.DrawTexture(TextureGenerator.TextureFromHeightMap(mapData.heightMap));
            else if (drawMode == DrawMode.ColorMap)
                display.DrawTexture(TextureGenerator.TextureFromColorMap(mapData.colorMap, mapChunkSize, mapChunkSize));
            else if (drawMode == DrawMode.Mesh) display.DrawMesh(MeshGenerator.GenerateTerrainMesh(mapData.heightMap, meshHeightMultiplier, meshHeightCurve, editorPreviewLOD), TextureGenerator.TextureFromColorMap(mapData.colorMap, mapChunkSize, mapChunkSize));
        }

        public void RequestMapData(Vector2 centre, Action<MapData> callback)
        {
            void ThreadStart()
            {
                MapDataThread(centre, callback);
            }

            new Thread(ThreadStart).Start();
        }

        private void MapDataThread(Vector2 centre, Action<MapData> callback)
        {
            var mapData = GenerateMapData(centre);
            lock (mapDataThreadInfoQueue)
            {
                mapDataThreadInfoQueue.Enqueue(new MapThreadInfo<MapData>(callback, mapData));
            }
        }

        public void RequestMeshData(MapData mapData, int lod, Action<MeshData> callback)
        {
            ThreadStart threadStart = delegate
            {
                MeshDataThread(mapData, lod, callback);
            };

            new Thread(threadStart).Start();
        }

        private void MeshDataThread(MapData mapData,int lod, Action<MeshData> callback)
        {
            var meshData = MeshGenerator.GenerateTerrainMesh(mapData.heightMap, meshHeightMultiplier, meshHeightCurve, lod);
            lock (meshDataThredInfoQueue)
            {
                meshDataThredInfoQueue.Enqueue(new MapThreadInfo<MeshData>(callback, meshData));
            }
        }

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
        private MapData GenerateMapData(Vector2 centre)
        {
            if (randomSeed) seed = new Random().Next(int.MinValue, int.MaxValue); // Set random seed if wanted.
            var noiseMap = Noise.GenerateNoiseMap(mapChunkSize, mapChunkSize, noiseScale, seed, octaves, persistence, lacunarity, centre + offset, normalizeMode); // Generate noise map.

            var colorMap = new Color[mapChunkSize * mapChunkSize];

            // Loop through every coordinate and get the current height value. Then we find our region which has its height value in range.
            // After that we assign the corresponding color to a 2d array of colors at the same coordinates we got the height from.
            for (var y = 0; y < mapChunkSize; y++) // Loop y
            for (var x = 0; x < mapChunkSize; x++) // Loop x
            {
                var currentHeight = noiseMap[x, y]; // Get height

                for (var i = 0; i < regions.Length; i++) // Find region
                {
                    if (currentHeight >= regions[i].height)
                    {
                        colorMap[y * mapChunkSize + x] = regions[i].color; // Assign color
                    }
                    else
                    {
                        break;
                    }
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
