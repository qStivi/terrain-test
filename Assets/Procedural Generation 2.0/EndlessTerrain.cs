using System.Collections.Generic;
using UnityEngine;

namespace Procedural_Generation_2._0
{
    public class EndlessTerrain : MonoBehaviour
    {
        private const float MAXViewDistance = 5000;

        private static Vector2 _viewerPosition;
        private static MapGenerator _mapGenerator;
        public Transform viewer;
        public Material mapMaterial;

        private readonly Dictionary<Vector2, TerrainChunk> _terrainChunkDictionary = new Dictionary<Vector2, TerrainChunk>();
        private readonly List<TerrainChunk> _terrainChunksVisibleLastUpdate = new List<TerrainChunk>();
        private int _chunkSize;
        private int _chunksVisibleInViewDistance;

        private void Start()
        {
            _mapGenerator = GetComponent<MapGenerator>();
            _chunkSize = MapGenerator.MapChunkSize - 1;
            _chunksVisibleInViewDistance = Mathf.RoundToInt(MAXViewDistance / _chunkSize);
        }

        private void Update()
        {
            var position = viewer.position;
            _viewerPosition = new Vector2(position.x, position.z);
            UpdateVisibleChunks();
        }

        private void UpdateVisibleChunks()
        {
            foreach (var terrainChunk in _terrainChunksVisibleLastUpdate)
                terrainChunk.SetVisible(false);

            _terrainChunksVisibleLastUpdate.Clear();

            var currentChunkCoordX = Mathf.RoundToInt(_viewerPosition.x / _chunkSize);
            var currentChunkCoordY = Mathf.RoundToInt(_viewerPosition.y / _chunkSize);

            for (var yOffset = -_chunksVisibleInViewDistance; yOffset <= _chunksVisibleInViewDistance; yOffset++)
            for (var xOffset = -_chunksVisibleInViewDistance; xOffset <= _chunksVisibleInViewDistance; xOffset++)
            {
                var viewedChunkCoord = new Vector2(currentChunkCoordX + xOffset, currentChunkCoordY + yOffset);

                if (_terrainChunkDictionary.ContainsKey(viewedChunkCoord))
                {
                    _terrainChunkDictionary[viewedChunkCoord].UpdateTerrainChunk();
                    if (_terrainChunkDictionary[viewedChunkCoord].IsVisible()) _terrainChunksVisibleLastUpdate.Add(_terrainChunkDictionary[viewedChunkCoord]);
                }
                else
                {
                    _terrainChunkDictionary.Add(viewedChunkCoord, new TerrainChunk(viewedChunkCoord, _chunkSize, transform, mapMaterial));
                }
            }
        }

        private class TerrainChunk
        {
            private readonly MeshFilter _meshFilter;
            private readonly GameObject _meshObject;
            private Bounds _bounds;

            public TerrainChunk(Vector2 coord, int size, Transform parent, Material material)
            {
                var position = coord * size;
                _bounds = new Bounds(position, Vector2.one * size);
                var positionV3 = new Vector3(position.x, 0, position.y);

                _meshObject = new GameObject("Terrain Chunk");
                var meshRenderer = _meshObject.AddComponent<MeshRenderer>();
                _meshFilter = _meshObject.AddComponent<MeshFilter>();
                meshRenderer.material = material;

                _meshObject.transform.position = positionV3;
                _meshObject.transform.parent = parent;
                SetVisible(false);

                _mapGenerator.RequestMapData(OnMapDataReceived);
            }

            private void OnMapDataReceived(MapData mapData)
            {
                _mapGenerator.RequestMeshData(mapData, OnMeshDataReceived);
            }

            private void OnMeshDataReceived(MeshData meshData)
            {
                _meshFilter.mesh = meshData.CreateMesh();
            }

            public void UpdateTerrainChunk()
            {
                var viewerDistanceFromNearestEdge = Mathf.Sqrt(_bounds.SqrDistance(_viewerPosition));
                var visible = viewerDistanceFromNearestEdge <= MAXViewDistance;
                SetVisible(visible);
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
    }
}
