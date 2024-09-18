using PixelatedDigging.Utilities;
using System.Collections.Generic;
using UnityEngine;

namespace PixelatedDigging
{
    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(MeshRenderer))]
    public class VoxelChunkSurface : MonoBehaviour
    {
        Mesh mesh;
        List<Vector3> vertices;
        List<int> triangles;

        int[] vertexRowCacheMin;
        int[] vertexRowCacheMax;

        float textureVoxelResolution;

        const int emptyVertexValue = -1;

        public void Initialize(Vector2Int resolution, Material material,
            float textureVoxelResolution)
        {
            this.textureVoxelResolution = textureVoxelResolution;

            GetComponent<MeshFilter>().mesh = mesh = new Mesh();
            mesh.name = "VoxelChunkSurfaceMesh";
            vertices = new List<Vector3>();
            triangles = new List<int>();

            vertexRowCacheMin = new int[resolution.x + 1];
            vertexRowCacheMin.Populate(emptyVertexValue);

            vertexRowCacheMax = new int[resolution.x + 1];
            vertexRowCacheMax.Populate(emptyVertexValue);

            GetComponent<MeshRenderer>().sharedMaterial = material;
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

        public void CacheCornerA(Vector3 a, int cellIndex)
        {
            vertexRowCacheMin[cellIndex] = vertices.Count;
            vertices.Add(a);
        }

        public void CacheCornerB(Vector3 b, int cellIndex)
        {
            vertexRowCacheMin[cellIndex + 1] = vertices.Count;
            vertices.Add(b);
        }

        public void CacheCornerC(Vector3 c, int cellIndex)
        {
            vertexRowCacheMax[cellIndex] = vertices.Count;
            vertices.Add(c);
        }

        public void CacheCornerD(Vector3 d, int cellIndex)
        {
            vertexRowCacheMax[cellIndex + 1] = vertices.Count;
            vertices.Add(d);
        }

        public void SwapRowCaches()
        {
            (vertexRowCacheMax, vertexRowCacheMin) = (vertexRowCacheMin, vertexRowCacheMax);

            vertexRowCacheMax.Populate(emptyVertexValue);
        }

        public void AddQuad(int i)
        {
            triangles.Add(vertexRowCacheMin[i]);
            triangles.Add(vertexRowCacheMax[i]);
            triangles.Add(vertexRowCacheMax[i + 1]);
            triangles.Add(vertexRowCacheMin[i]);
            triangles.Add(vertexRowCacheMax[i + 1]);
            triangles.Add(vertexRowCacheMin[i + 1]);
        }
    }
}