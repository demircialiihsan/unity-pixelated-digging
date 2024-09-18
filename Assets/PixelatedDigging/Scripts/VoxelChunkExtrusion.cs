using PixelatedDigging.Utilities;
using System.Collections.Generic;
using UnityEngine;

namespace PixelatedDigging
{
    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(MeshRenderer))]
    public class VoxelChunkExtrusion : MonoBehaviour
    {
        [SerializeField] VoxelChunkColliders colliders;

        Mesh mesh;
        List<Vector3> vertices;
        List<int> triangles;

        float extrusionHeight;

        int[] vertexRowCacheMin;
        int[] vertexRowCacheMax;

        float textureVoxelResolution;

        const int emptyVertexValue = -1;

        public void Initialize(Vector2Int resolution, float extrusionHeight, Material material,
            float textureVoxelResolution)
        {
            this.extrusionHeight = extrusionHeight;
            this.textureVoxelResolution = textureVoxelResolution;

            GetComponent<MeshFilter>().mesh = mesh = new Mesh();
            mesh.name = "VoxelChunkExtrusionMesh";
            vertices = new List<Vector3>();
            triangles = new List<int>();

            vertexRowCacheMin = new int[resolution.x + 1];
            vertexRowCacheMin.Populate(emptyVertexValue);

            vertexRowCacheMax = new int[resolution.x + 1];
            vertexRowCacheMax.Populate(emptyVertexValue);

            GetComponent<MeshRenderer>().sharedMaterial = material;

            colliders.Initialize();
        }

        void OnDestroy()
        {
            Destroy(mesh);
        }

        public void Clear()
        {
            vertices.Clear();
            triangles.Clear();
            mesh.Clear();

            vertexRowCacheMin.Populate(emptyVertexValue);
            vertexRowCacheMax.Populate(emptyVertexValue);

            colliders.Clear();
        }

        public void ApplyMesh()
        {
            mesh.vertices = vertices.ToArray();
            mesh.triangles = triangles.ToArray();
            mesh.RecalculateNormals();
        }

        public void ApplyUVs(Vector2 gridMin, Vector2 gridMax, Vector2Int gridResolution)
        {
            var uvs = new Vector2[vertices.Count];

            for (int i = 0; i < vertices.Count; i++)
            {
                var vertexWorld = transform.TransformPoint(vertices[i]);
                var percentX = Mathf.InverseLerp(gridMin.x, gridMax.x, vertexWorld.x) *
                    gridResolution.x / textureVoxelResolution;
                var percentY = Mathf.InverseLerp(gridMin.y, gridMax.y, vertexWorld.y) *
                    gridResolution.y / textureVoxelResolution;
                uvs[i] = new Vector2(percentX, percentY);
            }
            mesh.uv = uvs;
        }

        public void AddColliderPaths(int cellType, int cellIndex)
        {
            var a = vertexRowCacheMin[cellIndex];
            var b = vertexRowCacheMin[cellIndex + 1];
            var c = vertexRowCacheMax[cellIndex];
            var d = vertexRowCacheMax[cellIndex + 1];

            colliders.AddColliderPaths(cellType, a, b, c, d);
        }

        public void GenerateColliders()
        {
            colliders.GenerateColliders(vertices);
        }

        public bool IsCornerACached(int cellIndex)
        {
            return vertexRowCacheMin[cellIndex] != emptyVertexValue;
        }

        public bool IsCornerBCached(int cellIndex)
        {
            return vertexRowCacheMin[cellIndex + 1] != emptyVertexValue;
        }

        public bool IsCornerCCached(int cellIndex)
        {
            return vertexRowCacheMax[cellIndex] != emptyVertexValue;
        }

        public bool IsCornerDCached(int cellIndex)
        {
            return vertexRowCacheMax[cellIndex + 1] != emptyVertexValue;
        }

        public void CacheCornerA(Vector3 a, int cellIndex)
        {
            vertexRowCacheMin[cellIndex] = vertices.Count;
            vertices.Add(a);
            a.z = extrusionHeight;
            vertices.Add(a);
        }

        public void CacheCornerB(Vector3 b, int cellIndex)
        {
            vertexRowCacheMin[cellIndex + 1] = vertices.Count;
            vertices.Add(b);
            b.z = extrusionHeight;
            vertices.Add(b);
        }

        public void CacheCornerC(Vector3 c, int cellIndex)
        {
            vertexRowCacheMax[cellIndex] = vertices.Count;
            vertices.Add(c);
            c.z = extrusionHeight;
            vertices.Add(c);
        }

        public void CacheCornerD(Vector3 d, int cellIndex)
        {
            vertexRowCacheMax[cellIndex + 1] = vertices.Count;
            vertices.Add(d);
            d.z = extrusionHeight;
            vertices.Add(d);
        }

        public void SwapRowCaches()
        {
            (vertexRowCacheMax, vertexRowCacheMin) = (vertexRowCacheMin, vertexRowCacheMax);

            vertexRowCacheMax.Populate(emptyVertexValue);
        }

        public void AddSectionAB(int cellIndex)
        {
            AddSection(vertexRowCacheMin[cellIndex], vertexRowCacheMin[cellIndex + 1]);
        }

        public void AddSectionCA(int cellIndex)
        {
            AddSection(vertexRowCacheMax[cellIndex], vertexRowCacheMin[cellIndex]);
        }

        public void AddSectionDC(int cellIndex)
        {
            AddSection(vertexRowCacheMax[cellIndex + 1], vertexRowCacheMax[cellIndex]);
        }

        public void AddSectionBD(int cellIndex)
        {
            AddSection(vertexRowCacheMin[cellIndex + 1], vertexRowCacheMax[cellIndex + 1]);
        }

        void AddSection(int a, int b)
        {
            triangles.Add(a);
            triangles.Add(b);
            triangles.Add(b + 1);
            triangles.Add(a);
            triangles.Add(b + 1);
            triangles.Add(a + 1);
        }
    }
}