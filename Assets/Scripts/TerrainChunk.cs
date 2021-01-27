using System;
using Data;
using UnityEngine;

public class TerrainChunk : ScriptableObject
{
    private const float ColliderGenerationDistanceThreshold = 5;
    public Vector2 coord;
    private Bounds _bounds;
    private int _colliderLODIndex;

    private LODInfo[] _detailLevels;
    private bool _hasSetCollider;

    private HeightMap _heightMap;
    private bool _heightMapReceived;

    private LODMesh[] _lodMeshes;
    private float _maxViewDst;
    private MeshCollider _meshCollider;
    private MeshFilter _meshFilter;

    private GameObject _meshObject;

    private MeshRenderer _meshRenderer;
    private MeshSettings _meshSettings;
    private int _previousLODIndex = -1;
    private Vector2 _sampleCentre;
    private Transform _viewer;

    private Vector2 ViewerPosition => new Vector2(_viewer.position.x, _viewer.position.z);

    public event Action<TerrainChunk, bool> ONVisibilityChanged;

    public void Load(Vector2 terrainCoord, HeightMapSettings heightMapSettings, MeshSettings meshSettings, LODInfo[] detailLevels, int colliderLODIndex, Transform viewer, Transform parent, Material material)
    {
        coord = terrainCoord;
        _detailLevels = detailLevels;
        _colliderLODIndex = colliderLODIndex;
        _meshSettings = meshSettings;
        _viewer = viewer;

        _sampleCentre = terrainCoord * meshSettings.MeshWorldSize / meshSettings.meshScale;
        var position = terrainCoord * meshSettings.MeshWorldSize;
        _bounds = new Bounds(position, Vector2.one * meshSettings.MeshWorldSize);

        _meshObject = new GameObject("Terrain Chunk");
        _meshRenderer = _meshObject.AddComponent<MeshRenderer>();
        _meshFilter = _meshObject.AddComponent<MeshFilter>();
        _meshCollider = _meshObject.AddComponent<MeshCollider>();
        _meshRenderer.material = material;

        _meshObject.transform.position = new Vector3(position.x, 0, position.y);
        _meshObject.transform.parent = parent;
        SetVisible(false);

        _lodMeshes = new LODMesh[detailLevels.Length];
        for (var i = 0; i < detailLevels.Length; i++)
        {
            _lodMeshes[i] = new LODMesh(detailLevels[i].lod);
            _lodMeshes[i].UpdateCallback += UpdateTerrainChunk;
            if (i == colliderLODIndex) _lodMeshes[i].UpdateCallback += UpdateCollisionMesh;
        }

        _maxViewDst = detailLevels[detailLevels.Length - 1].visibleDstThreshold;


        ThreadedDataRequester.RequestData(() => HeightMapGenerator.GenerateHeightMap(meshSettings.NumVertsPerLine, meshSettings.NumVertsPerLine, heightMapSettings, _sampleCentre), OnHeightMapReceived);
    }

    private void OnHeightMapReceived(object heightMapObject)
    {
        _heightMap = (HeightMap) heightMapObject;
        _heightMapReceived = true;

        UpdateTerrainChunk();
    }

    public void UpdateTerrainChunk()
    {
        if (!_heightMapReceived) return;
        var viewerDistanceFromNearestEdge = Mathf.Sqrt(_bounds.SqrDistance(ViewerPosition));

        var wasVisible = IsVisible();
        var visible = viewerDistanceFromNearestEdge <= _maxViewDst;

        if (visible)
        {
            var lodIndex = 0;

            for (var i = 0; i < _detailLevels.Length - 1; i++)
                if (viewerDistanceFromNearestEdge > _detailLevels[i].visibleDstThreshold)
                    lodIndex = i + 1;
                else
                    break;

            if (lodIndex != _previousLODIndex)
            {
                var lodMesh = _lodMeshes[lodIndex];
                if (lodMesh.HasMesh)
                {
                    _previousLODIndex = lodIndex;
                    _meshFilter.mesh = lodMesh.Mesh;
                }
                else if (!lodMesh.HasRequestedMesh)
                {
                    lodMesh.RequestMesh(_heightMap, _meshSettings);
                }
            }
        }

        if (wasVisible == visible) return;
        SetVisible(visible);
        ONVisibilityChanged?.Invoke(this, visible);
    }

    // TODO Optimize this! Maybe make it threaded.
    public void UpdateCollisionMesh()
    {
        if (_hasSetCollider) return;
        var sqrDstFromViewerToEdge = _bounds.SqrDistance(ViewerPosition);

        if (sqrDstFromViewerToEdge < _detailLevels[_colliderLODIndex].SqrVisibleDstThreshold)
            if (!_lodMeshes[_colliderLODIndex].HasRequestedMesh)
                _lodMeshes[_colliderLODIndex].RequestMesh(_heightMap, _meshSettings);

        if (!(sqrDstFromViewerToEdge < ColliderGenerationDistanceThreshold * ColliderGenerationDistanceThreshold)) return;
        if (!_lodMeshes[_colliderLODIndex].HasMesh) return;
        _meshCollider.sharedMesh = _lodMeshes[_colliderLODIndex].Mesh;
        _hasSetCollider = true;
    }

    public void SetVisible(bool visible)
    {
        _meshObject.SetActive(visible);
    }

    public bool IsVisible()
    {
        return _meshObject.activeSelf;
    }
}

internal class LODMesh
{
    private readonly int _lod;
    public bool HasMesh;
    public bool HasRequestedMesh;
    public Mesh Mesh;

    public LODMesh(int lod)
    {
        _lod = lod;
    }

    public event Action UpdateCallback;

    private void OnMeshDataReceived(object meshDataObject)
    {
        Mesh = ((MeshData) meshDataObject).CreateMesh();
        HasMesh = true;

        UpdateCallback?.Invoke();
    }

    public void RequestMesh(HeightMap heightMap, MeshSettings meshSettings)
    {
        HasRequestedMesh = true;
        ThreadedDataRequester.RequestData(() => MeshGenerator.GenerateTerrainMesh(heightMap.Values, meshSettings, _lod), OnMeshDataReceived);
    }
}
