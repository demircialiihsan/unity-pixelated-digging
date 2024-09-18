using UnityEngine;

namespace PixelatedDigging
{
    public class DigController : MonoBehaviour
    {
        [SerializeField] int stencilRadius;

        VoxelGrid grid;

        float voxelSize;
        Vector2Int chunkResolution;
        Vector2 halfGridSize;

        public void Prepare(VoxelGrid grid, float voxelSize, Vector2Int chunkResolution,
            Vector2 gridSize)
        {
            this.grid = grid;
            this.voxelSize = voxelSize;
            this.chunkResolution = chunkResolution;
            halfGridSize = 0.5f * gridSize;

            CreateGridHitCollider(gridSize);
        }

        void CreateGridHitCollider(Vector2 boardSize)
        {
            var collider = gameObject.AddComponent<BoxCollider>();
            collider.size = boardSize;
            collider.isTrigger = true;
        }

        void Update()
        {
            if (Input.GetMouseButton(0))
            {
                if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition),
                    out RaycastHit hit))
                {
                    if (hit.collider.gameObject == gameObject)
                    {
                        RegisterGridHit(hit.point);
                    }
                }
            }
        }

        public void RegisterGridHit(Vector3 hitWorldPoint)
        {
            var (chunkCoord, voxelCoord) = GetCoordFromLocalPoint(transform.InverseTransformPoint(
                hitWorldPoint));

            var stencil = new VoxelStencil(stencilRadius);
            grid.Dig(chunkCoord, voxelCoord, stencil);
        }

        (Vector2Int, Vector2Int) GetCoordFromLocalPoint(Vector2 localPoint)
        {
            var pointLocalPosOnBoard = localPoint + halfGridSize; //relative to the down-leftmost point

            var voxelX = (int)(pointLocalPosOnBoard.x / voxelSize);
            var voxelY = (int)(pointLocalPosOnBoard.y / voxelSize);

            var chunkX = voxelX / chunkResolution.x;
            var chunkY = voxelY / chunkResolution.y;

            voxelX -= chunkX * chunkResolution.x;
            voxelY -= chunkY * chunkResolution.y;

            return (new Vector2Int(chunkX, chunkY), new Vector2Int(voxelX, voxelY));
        }
    }
}