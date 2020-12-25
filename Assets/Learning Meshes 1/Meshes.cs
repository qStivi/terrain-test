using UnityEngine;

// The most basic mesh is a triangle.
// The most basic triangle is made of 3 vertices, the order in which they are read and the direction in which it is facing.
// A mesh can be made out of more than one triangle. To define the shape of such a mesh we need to tell unity in which

namespace Learning_Meshes_1
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
            _vertices = new[] {new Vector3(0, 0, 0), new Vector3(0, 0, 1), new Vector3(1, 0, 0)};
            _triangles = new[] {0, 1, 2};
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