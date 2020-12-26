using UnityEngine;

namespace Procedual_generation
{
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public class MeshGenerator : MonoBehaviour
    {
        public int xSize = 20;
        public int zSize = 20;

        private Mesh _mesh;
        private int[] _triangles;
        private Vector3[] _vertices;

        private void Start()
        {
            _mesh = new Mesh();

            GetComponent<MeshFilter>().mesh = _mesh;

            CreateShape();
            UpdateShape();
        }
        
        private void CreateShape()
        {
            _vertices = new Vector3[(xSize + 1) * (zSize + 1)];

            for (int i = 0, z = 0; z <= zSize; z++)
            for (var x = 0; x <= xSize; x++)
            {
                var y = Mathf.PerlinNoise(x * .3f, z * .3f) * 2f;
                _vertices[i] = new Vector3(x, y, z);
                i++;
            }

            _triangles = new int[xSize * zSize * 6];

            var vert = 0;
            var tris = 0;

            for (var z = 0; z < zSize; z++)
            {
                for (var x = 0; x < xSize; x++)
                {
                    _triangles[tris + 0] = vert + 0;
                    _triangles[tris + 1] = vert + xSize + 1;
                    _triangles[tris + 2] = vert + 1;

                    _triangles[tris + 3] = vert + 1;
                    _triangles[tris + 4] = vert + xSize + 1;
                    _triangles[tris + 5] = vert + xSize + 2;

                    vert++;
                    tris += 6;
                }

                vert++;
            }
        }

        private void UpdateShape()
        {
            _mesh.Clear();

            _mesh.vertices = _vertices;
            _mesh.triangles = _triangles;

            _mesh.RecalculateNormals();
        }
    }
}