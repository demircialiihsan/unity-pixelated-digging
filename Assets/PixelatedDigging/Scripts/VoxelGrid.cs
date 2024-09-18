using PixelatedDigging.FX;
using UnityEngine;

namespace PixelatedDigging
{
    public class VoxelGrid : MonoBehaviour
    {
        [SerializeField] float voxelSize;
        [SerializeField] Vector2Int chunkResolution;
        [SerializeField] Vector2Int gridResolution;
        [SerializeField] float extrusionHeight;
        [Space]
        [SerializeField] VoxelChunk chunkPrefab;
        [Space]
        [SerializeField] DigController digController;
        [SerializeField] VoxelGridDigFXHandler digFXHandler;

        VoxelChunk[,] chunks;

        public void Initialize(Material material, float textureVoxelResolution)
        {
            chunks = new VoxelChunk[gridResolution.x, gridResolution.y];

            var chunkSize = voxelSize * (Vector2)chunkResolution;
            var halfChunkSize = 0.5f * chunkSize;
            var halfGridSize = halfChunkSize * (Vector2)gridResolution;
            var originChunkLocalPos = -halfGridSize + halfChunkSize;

            for (int y = 0; y < chunks.GetLength(1); y++)
            {
                var gridMin = (Vector2)transform.position - halfGridSize;
                var gridMax = (Vector2)transform.position + halfGridSize;
                var gridVoxelResolution = gridResolution * chunkResolution;

                for (int x = 0; x < chunks.GetLength(0); x++)
                {
                    CreateChunk(x, y, gridMin, gridMax, gridVoxelResolution);
                }
            }

            var gridSize = halfGridSize * 2;
            digController.Prepare(this, voxelSize, chunkResolution, gridSize);

            digFXHandler.Initialize(voxelSize, voxelSize, material, textureVoxelResolution);

            void CreateChunk(int x, int y, Vector2 gridMin, Vector2 gridMax,
                Vector2Int gridVoxelResolution)
            {
                var localPos = chunkSize * new Vector2(x, y); // relative pos to origin chunk pos

                var chunk = Instantiate(chunkPrefab);
                chunk.transform.SetParent(transform);
                chunk.transform.localPosition = originChunkLocalPos + localPos;

                chunk.Initialize(voxelSize, chunkResolution, extrusionHeight, material,
                    textureVoxelResolution, gridMin, gridMax, gridVoxelResolution, digFXHandler);
                chunks[x, y] = chunk;
            }
        }

        public void Dig(Vector2Int chunkCoord, Vector2Int voxelCoord, VoxelStencil stencil)
        {
            if (chunkCoord.x < 0 || chunkCoord.x >= chunks.GetLength(0) ||
                chunkCoord.y < 0 || chunkCoord.y >= chunks.GetLength(1))
                return;

            // grid relative voxel coords
            var gridVoxelCoordX = chunkResolution.x * chunkCoord.x + voxelCoord.x;
            var gridVoxelCoordY = chunkResolution.y * chunkCoord.y + voxelCoord.y;

            var chunkMinX = (gridVoxelCoordX - stencil.Extents) / chunkResolution.x;
            chunkMinX = Mathf.Max(0, chunkMinX);
            var chunkMaxX = (gridVoxelCoordX + stencil.Extents) / chunkResolution.x;
            chunkMaxX = Mathf.Min(chunkMaxX, gridResolution.x - 1);

            var chunkMinY = (gridVoxelCoordY - stencil.Extents) / chunkResolution.y;
            chunkMinY = Mathf.Max(0, chunkMinY);
            var chunkMaxY = (gridVoxelCoordY + stencil.Extents) / chunkResolution.y;
            chunkMaxY = Mathf.Min(chunkMaxY, gridResolution.y - 1);

            for (int y = chunkMinY; y <= chunkMaxY; y++)
            {
                var chunkOffsetY = y - chunkCoord.y;

                for (int x = chunkMinX; x <= chunkMaxX; x++)
                {
                    var chunkOffsetX = x - chunkCoord.x;

                    var offset = new Vector2Int(chunkOffsetX * chunkResolution.x,
                        chunkOffsetY * chunkResolution.y);

                    stencil.SetCenter(voxelCoord - offset);
                    chunks[x, y].Apply(stencil);
                }
            }
        }

#if UNITY_EDITOR
        void OnDrawGizmos()
        {
            Gizmos.color = Color.cyan;
            Vector3 gridSize = voxelSize * ((Vector2)chunkResolution * gridResolution);
            gridSize.z = extrusionHeight;

            var pos = transform.position + extrusionHeight * 0.5f * Vector3.forward;
            Gizmos.DrawWireCube(pos, gridSize);
        }
#endif
    }
}