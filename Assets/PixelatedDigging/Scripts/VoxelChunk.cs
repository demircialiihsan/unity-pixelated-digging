using PixelatedDigging.FX;
using UnityEngine;

namespace PixelatedDigging
{
    public class VoxelChunk : MonoBehaviour
    {
        [SerializeField] VoxelChunkSurface surfacePrefab;
        [SerializeField] VoxelChunkExtrusion extrusionPrefab;

        VoxelChunkSurface surface;
        VoxelChunkExtrusion extrusion;

        Voxel[,] voxels;

        Vector2 gridMin;
        Vector2 gridMax;
        Vector2Int gridResolution;

        VoxelGridDigFXHandler fxHandler;

        public void Initialize(float voxelSize, Vector2Int resolution, float extrusionHeight,
            Material material, float textureVoxelResolution, Vector2 gridMin, Vector2 gridMax,
            Vector2Int gridResolution, VoxelGridDigFXHandler fxHandler)
        {
            voxels = new Voxel[resolution.x, resolution.y];
            this.gridMin = gridMin;
            this.gridMax = gridMax;
            this.gridResolution = gridResolution;
            this.fxHandler = fxHandler;

            var halfVoxelSize = 0.5f * voxelSize * Vector2.one;
            var halfChunkSize = halfVoxelSize * (Vector2)resolution;
            var originVoxelLocalPos = -halfChunkSize + halfVoxelSize;

            for (int y = 0; y < voxels.GetLength(1); y++)
            {
                for (int x = 0; x < voxels.GetLength(0); x++)
                {
                    CreateVoxel(x, y);
                }
            }

            surface = Instantiate(surfacePrefab, transform.position, Quaternion.identity,
                transform);
            surface.Initialize(resolution, material, textureVoxelResolution);

            extrusion = Instantiate(extrusionPrefab, transform.position, Quaternion.identity,
                transform);
            extrusion.Initialize(resolution, extrusionHeight, material, textureVoxelResolution);

            Refresh();

            void CreateVoxel(int x, int y)
            {
                var localPos = voxelSize * new Vector2(x, y); // relative pos to origin voxel pos
                voxels[x, y] = new Voxel(originVoxelLocalPos + localPos, voxelSize);
            }
        }

        public void Apply(VoxelStencil stencil)
        {
            var xMin = Mathf.Max(0, stencil.XMin);
            var xMax = Mathf.Min(voxels.GetLength(0) - 1, stencil.XMax);
            var yMin = Mathf.Max(0, stencil.YMin);
            var yMax = Mathf.Min(voxels.GetLength(1) - 1, stencil.YMax);

            bool refreshFlag = false;

            for (int y = yMin; y <= yMax; y++)
            {
                for (int x = xMin; x <= xMax; x++)
                {
                    var voxel = voxels[x, y];
                    var currentFill = voxels[x, y].IsFilled;
                    var newFill = stencil.Apply(x, y, currentFill);

                    if (newFill != currentFill)
                    {
                        voxel.IsFilled = newFill;
                        if (newFill == false)
                        {
                            var pos = transform.TransformPoint(voxel.Position);
                            fxHandler.ShowEffect(pos, gridMin, gridMax, gridResolution);
                        }
                        refreshFlag = true;
                    }
                }
            }

            if (refreshFlag)
                Refresh();
        }

        void Refresh()
        {
            surface.Clear();
            extrusion.Clear();

            TriangulateCells();

            surface.ApplyMesh();
            extrusion.ApplyMesh();

            surface.ApplyUVs(gridMin, gridMax, gridResolution);
            extrusion.ApplyUVs(gridMin, gridMax, gridResolution);

            extrusion.GenerateColliders();
        }

        void TriangulateCells()
        {
            for (int y = 0; y < voxels.GetLength(1); y++)
            {
                for (int x = 0; x < voxels.GetLength(0); x++)
                {
                    if (voxels[x, y].IsFilled)
                    {
                        var cellType = GetCellType(x, y);

                        CacheCell(x, y, cellType);
                        TriangulateCell(x, cellType);
                        extrusion.AddColliderPaths((int)cellType, x);
                    }
                }
                SwapRowCaches();
            }
        }

        CellType GetCellType(int x, int y)
        {
            CellType cellType = 0;

            if (IsDownNeighborFilled())
                cellType |= CellType.DownFilled;
            if (IsLeftNeighborFilled())
                cellType |= CellType.LeftFilled;
            if (IsUpNeighborFilled())
                cellType |= CellType.UpFilled;
            if (IsRightNeighborFilled())
                cellType |= CellType.RightFilled;

            return cellType;

            bool IsDownNeighborFilled()
            {
                if (y > 0)
                    return voxels[x, y - 1].IsFilled;
                return false;
            }

            bool IsLeftNeighborFilled()
            {
                if (x > 0)
                    return voxels[x - 1, y].IsFilled;
                return false;
            }

            bool IsRightNeighborFilled()
            {
                if (x < voxels.GetLength(0) - 1)
                    return voxels[x + 1, y].IsFilled;
                return false;
            }

            bool IsUpNeighborFilled()
            {
                if (y < voxels.GetLength(1) - 1)
                    return voxels[x, y + 1].IsFilled;
                return false;
            }
        }

        void CacheCell(int x, int y, CellType cellType)
        {
            var voxel = voxels[x, y];

            // cache surface points
            if (!surface.IsCornerACached(x))
                surface.CacheCornerA(voxel.A, x);

            if (!surface.IsCornerBCached(x))
                surface.CacheCornerB(voxel.B, x);

            if (!surface.IsCornerCCached(x))
                surface.CacheCornerC(voxel.C, x);

            // since iteration starts from down-left cell and then goes horizontal first, always add top-right corner without checking
            surface.CacheCornerD(voxel.D, x);

            // cache extrusion points
            if (!cellType.HasFlag(CellType.DownFilled))
            {
                // down cell is empty, cache AB extrusion points
                if (!extrusion.IsCornerACached(x))
                    extrusion.CacheCornerA(voxel.A, x);
                if (!extrusion.IsCornerBCached(x))
                    extrusion.CacheCornerB(voxel.B, x);
            }
            if (!cellType.HasFlag(CellType.LeftFilled))
            {
                // left cell is empty, cache AC extrusion points
                if (!extrusion.IsCornerACached(x))
                    extrusion.CacheCornerA(voxel.A, x);
                if (!extrusion.IsCornerCCached(x))
                    extrusion.CacheCornerC(voxel.C, x);
            }
            if (!cellType.HasFlag(CellType.UpFilled))
            {
                // up cell is empty, cache CD extrusion points
                if (!extrusion.IsCornerCCached(x))
                    extrusion.CacheCornerC(voxel.C, x);
                if (!extrusion.IsCornerDCached(x))
                    extrusion.CacheCornerD(voxel.D, x);
            }
            if (!cellType.HasFlag(CellType.RightFilled))
            {
                // right cell is empty, cache BD extrusion points
                if (!extrusion.IsCornerBCached(x))
                    extrusion.CacheCornerB(voxel.B, x);
                if (!extrusion.IsCornerDCached(x))
                    extrusion.CacheCornerD(voxel.D, x);
            }
        }

        void TriangulateCell(int x, CellType cellType)
        {
            surface.AddQuad(x);

            if (!cellType.HasFlag(CellType.DownFilled))
                extrusion.AddSectionAB(x);
            if (!cellType.HasFlag(CellType.LeftFilled))
                extrusion.AddSectionCA(x);
            if (!cellType.HasFlag(CellType.UpFilled))
                extrusion.AddSectionDC(x);
            if (!cellType.HasFlag(CellType.RightFilled))
                extrusion.AddSectionBD(x);
        }

        void SwapRowCaches()
        {
            surface.SwapRowCaches();
            extrusion.SwapRowCaches();
        }
    }
}