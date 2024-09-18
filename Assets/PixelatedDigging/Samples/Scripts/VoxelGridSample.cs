using UnityEngine;

namespace PixelatedDigging.Samples
{
    public class VoxelGridSample : MonoBehaviour
    {
        [SerializeField] VoxelGrid voxelGrid;
        [SerializeField] Material voxelGridMaterial;
        [SerializeField] float textureVoxelResolution = 16f;

        void Start()
        {
            voxelGrid.Initialize(voxelGridMaterial, textureVoxelResolution);
        }
    }
}