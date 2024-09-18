using UnityEngine;

namespace PixelatedDigging
{
    public class Voxel
    {
        public bool IsFilled;

        readonly Vector2 position;
        readonly float size;

        /// <summary>
        /// Center of the voxel.
        /// </summary>
        public Vector2 Position => position;

        /// <summary>
        /// Down-left corner of the voxel.
        /// </summary>
        public Vector2 A => position + 0.5f * new Vector2(-size, -size);

        /// <summary>
        /// Down-right corner of the voxel.
        /// </summary>
        public Vector2 B => position + 0.5f * new Vector2(size, -size);

        /// <summary>
        /// Up-left corner of the voxel.
        /// </summary>
        public Vector2 C => position + 0.5f * new Vector2(-size, size);

        /// <summary>
        /// Up-right corner of the voxel.
        /// </summary>
        public Vector2 D => position + 0.5f * new Vector2(size, size);

        public Voxel(Vector2 position, float size)
        {
            IsFilled = true;

            this.position = position;
            this.size = size;
        }
    }
}