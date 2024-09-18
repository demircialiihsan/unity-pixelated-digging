using UnityEngine;

namespace PixelatedDigging
{
    public class VoxelStencil
    {
        public readonly int Extents;
        readonly int radius;

        Vector2Int center;

        public VoxelStencil(int radius)
        {
            Extents = radius;
            this.radius = radius;
        }

        public int XMin => center.x - Extents;

        public int XMax => center.x + Extents;

        public int YMin => center.y - Extents;

        public int YMax => center.y + Extents;

        public bool Apply(int x, int y, bool currentVoxelState)
        {
            x -= center.x;
            y -= center.y;

            if (x * x + y * y <= radius * radius)
                return false;
            else
                return currentVoxelState;
        }

        public void SetCenter(Vector2Int center)
        {
            this.center = center;
        }
    }
}