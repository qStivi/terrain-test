using System;
using System.Collections.Generic;
using System.Threading;
using Data;
using UnityEngine;
using Random = System.Random;
using TerrainData = Data.TerrainData;

[RequireComponent(typeof(MapDisplay))]
public class MapGenerator : MonoBehaviour
{
    public enum DrawMode
    {
        NoiseMap,
        Mesh,
        FalloffMap
    }
    public DrawMode drawMode;

    public TerrainData terrainData;
    public NoiseData noiseData;
    public TextureData textureData;

    public Material terrainMaterial;

    [Range(0, 6)] public int editorPreviewLOD;


    public bool autoUpdate = true;

    public bool randomSeed;

    private readonly Queue<MapThreadInfo<MapData>> mapDataThreadInfoQueue = new Queue<MapThreadInfo<MapData>>();
    private readonly Queue<MapThreadInfo<MeshData>> meshDataThredInfoQueue = new Queue<MapThreadInfo<MeshData>>();

    private float[,] falloffMap;

    void OnValuesUpdated()
    {
        if (!Application.isPlaying)
        {
            DrawMapInEditor();
        }
    }

    void OnTextureValuesUpdated()
    {
        textureData.ApplyToMaterial(terrainMaterial);
    }

    public int mapChunkSize
    {
        get
        {
            if (terrainData.usingFlatShading)
            {
                return 95;
            }
            else
            {
                return 239;
            }
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

    private void OnValidate()
    {
        if (terrainData != null)
        {
            terrainData.OnValuesUpdated -= OnValuesUpdated;
            terrainData.OnValuesUpdated += OnValuesUpdated;
        }
        if (noiseData != null)
        {
            noiseData.OnValuesUpdated -= OnValuesUpdated;
            noiseData.OnValuesUpdated += OnValuesUpdated;
        }
        if (textureData != null)
        {
            textureData.OnValuesUpdated -= OnTextureValuesUpdated;
            textureData.OnValuesUpdated += OnTextureValuesUpdated;
        }
    }

    // Visualize noise map.
    public void DrawMapInEditor()
    {
        var mapData = GenerateMapData(Vector2.zero);

        var display = FindObjectOfType<MapDisplay>();
        switch (drawMode)
        {
            case DrawMode.NoiseMap:
                display.DrawTexture(TextureGenerator.TextureFromHeightMap(mapData.heightMap));
                break;
            case DrawMode.Mesh:
                display.DrawMesh(MeshGenerator.GenerateTerrainMesh(mapData.heightMap, terrainData.meshHeightMultiplier, terrainData.meshHeightCurve, editorPreviewLOD, terrainData.usingFlatShading));
                break;
            case DrawMode.FalloffMap:
                display.DrawTexture(TextureGenerator.TextureFromHeightMap(FalloffGenerator.GenerateFalloffMap(mapChunkSize)));
                break;
        }
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
        ThreadStart threadStart = delegate { MeshDataThread(mapData, lod, callback); };

        new Thread(threadStart).Start();
    }

    private void MeshDataThread(MapData mapData, int lod, Action<MeshData> callback)
    {
        var meshData = MeshGenerator.GenerateTerrainMesh(mapData.heightMap, terrainData.meshHeightMultiplier, terrainData.meshHeightCurve, lod, terrainData.usingFlatShading);
        lock (meshDataThredInfoQueue)
        {
            meshDataThredInfoQueue.Enqueue(new MapThreadInfo<MeshData>(callback, meshData));
        }
    }

    private MapData GenerateMapData(Vector2 centre)
    {
        if (randomSeed) noiseData.seed = new Random().Next(int.MinValue, int.MaxValue); // Set random seed if wanted.
        var noiseMap = Noise.GenerateNoiseMap(mapChunkSize + 2, mapChunkSize + 2, noiseData.noiseScale, noiseData.seed, noiseData.octaves, noiseData.persistence, noiseData.lacunarity, centre + noiseData.offset, noiseData.normalizeMode); // Generate noise map.

        if (terrainData.usingFalloff)
        {
            if (falloffMap == null)
            {
                falloffMap = FalloffGenerator.GenerateFalloffMap(mapChunkSize +2 );
            }
            
            // Loop through every coordinate and get the current height value. Then we find our region which has its height value in range.
            // After that we assign the corresponding color to a 2d array of colors at the same coordinates we got the height from.
            for (var y = 0; y < mapChunkSize + 2; y++) // Loop y
            for (var x = 0; x < mapChunkSize + 2; x++) // Loop x
            {
                if (terrainData.usingFalloff) noiseMap[x, y] = Mathf.Clamp01(noiseMap[x, y] - falloffMap[x, y]);
            }
        }
        
        textureData.UpdateMeshHeights(terrainMaterial, terrainData.minHeight, terrainData.maxHeight);

        return new MapData(noiseMap);
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

public struct MapData
{
    public MapData(float[,] heightMap)
    {
        this.heightMap = heightMap;
    }

    public readonly float[,] heightMap;
}
