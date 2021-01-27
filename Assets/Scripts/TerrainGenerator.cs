using System;
using System.Collections.Generic;
using Data;
using UnityEngine;

public class TerrainGenerator : MonoBehaviour
{
    private const float ViewerMoveThresholdForChunkUpdate = 25f;
    private const float SqrViewerMoveThresholdForChunkUpdate = ViewerMoveThresholdForChunkUpdate * ViewerMoveThresholdForChunkUpdate;

    public int colliderLODIndex;
    public LODInfo[] detailLevels;

    public MeshSettings meshSettings;
    public HeightMapSettings heightMapSettings;
    public TextureData textureSettings;

    public Transform viewer;
    public Material mapMaterial;

    public Vector2 viewerPosition;

    private readonly Dictionary<Vector2, TerrainChunk> _terrainChunkDictionary = new Dictionary<Vector2, TerrainChunk>();
    private readonly List<TerrainChunk> _visibleTerrainChunks = new List<TerrainChunk>();
    private int _chunksVisibleInViewDst;
    private float _meshWorldSize;
    private Vector2 _viewerPositionOld;

    private void Start()
    {
        textureSettings.ApplyToMaterial(mapMaterial);
        textureSettings.UpdateMeshHeights(mapMaterial, heightMapSettings.MINHeight, heightMapSettings.MAXHeight);

        var maxViewDst = detailLevels[detailLevels.Length - 1].visibleDstThreshold;
        _meshWorldSize = meshSettings.MeshWorldSize;
        _chunksVisibleInViewDst = Mathf.RoundToInt(maxViewDst / _meshWorldSize);

        UpdateVisibleChunks();
    }

    private void Update()
    {
        var position = viewer.position;
        viewerPosition = new Vector2(position.x, position.z);

        if (viewerPosition != _viewerPositionOld)
            foreach (var chunk in _visibleTerrainChunks)
                chunk.UpdateCollisionMesh();

        if (!((_viewerPositionOld - viewerPosition).sqrMagnitude > SqrViewerMoveThresholdForChunkUpdate)) return;
        _viewerPositionOld = viewerPosition;
        UpdateVisibleChunks();
    }

    private void UpdateVisibleChunks()
    {
        var alreadyUpdatedChunkCoords = new HashSet<Vector2>();

        for (var i = _visibleTerrainChunks.Count - 1; i >= 0; i--)
        {
            alreadyUpdatedChunkCoords.Add(_visibleTerrainChunks[i].coord);
            _visibleTerrainChunks[i].UpdateTerrainChunk();
        }

        var currentChunkCoordX = Mathf.RoundToInt(viewerPosition.x / _meshWorldSize);
        var currentChunkCoordY = Mathf.RoundToInt(viewerPosition.y / _meshWorldSize);

        for (var yOffset = -_chunksVisibleInViewDst; yOffset <= _chunksVisibleInViewDst; yOffset++)
        for (var xOffset = -_chunksVisibleInViewDst; xOffset <= _chunksVisibleInViewDst; xOffset++)
        {
            var viewedChunkCoord = new Vector2(currentChunkCoordX + xOffset, currentChunkCoordY + yOffset);
            if (alreadyUpdatedChunkCoords.Contains(viewedChunkCoord)) continue;
            if (_terrainChunkDictionary.ContainsKey(viewedChunkCoord))
            {
                _terrainChunkDictionary[viewedChunkCoord].UpdateTerrainChunk();
            }
            else
            {
                var newChunk = ScriptableObject.CreateInstance<TerrainChunk>();
                _terrainChunkDictionary.Add(viewedChunkCoord, newChunk);
                newChunk.ONVisibilityChanged += OnTerrainChunkVisibilityChanged;
                newChunk.Load(viewedChunkCoord, heightMapSettings, meshSettings, detailLevels, colliderLODIndex, viewer, transform, mapMaterial);
            }
        }
    }

    private void OnTerrainChunkVisibilityChanged(TerrainChunk chunk, bool isVisible)
    {
        if (isVisible)
            _visibleTerrainChunks.Add(chunk);
        else
            _visibleTerrainChunks.Remove(chunk);
    }
}


[Serializable]
public struct LODInfo
{
    [Range(0, MeshSettings.NumSupportedLODs - 1)]
    public int lod;

    public float visibleDstThreshold;

    public float SqrVisibleDstThreshold => visibleDstThreshold * visibleDstThreshold;
}
