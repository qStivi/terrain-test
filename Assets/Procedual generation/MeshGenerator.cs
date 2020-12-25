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

        private void OnDrawGizmos()
        {
            if (_vertices == null) return;

            foreach (var v in _vertices) Gizmos.DrawSphere(v, .1f);
        }

        private void CreateShape()
        {
            _vertices = new Vector3[(xSize + 1) * (zSize + 1)];

            for (int i = 0, z = 0; z <= zSize; z++)
            for (var x = 0; x <= xSize; x++)
            {
                _vertices[i] = new Vector3(x, 0, z);
                i++;
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