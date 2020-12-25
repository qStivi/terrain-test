using UnityEngine;

// The most basic mesh is a triangle.
// The most basic triangle is made of 3 vertices, the order in which they are read and the direction in which it is facing.
// The integer array tells unity what vertices to connect. Unity takes every 3 indices to form triangles.

namespace Learning_Meshes_2
{
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public class Meshes : MonoBehaviour
    {
        private Mesh _mesh;
        private int[] _triangles;

        private Vector3[] _vertices;

        private void Awake()
        {
            _mesh = GetComponent<MeshFilter>().mesh;
        }

        private void Start()
        {
            CreateMeshData();
            CreateMesh();
        }

        private void CreateMeshData()
        {
            // To create a quad we can use 4 vertices. We will create 2 triangles which will share 2 vertices. We also have to make sure that
            // we keep the vertices in an clockwise order because for performance reasons unity will render only the top size which is defined
            // by the order of which the vertices are read.
            _vertices = new[] {new Vector3(0, 0, 0), new Vector3(0, 0, 1), new Vector3(1, 0, 0), new Vector3(1, 0, 1)};
            _triangles = new[] {0, 1, 2, 2, 1, 3};
        }

        private void CreateMesh()
        {
            _mesh.Clear();
            _mesh.vertices = _vertices;
            _mesh.triangles = _triangles;
            _mesh.RecalculateNormals();
        }
    }
}