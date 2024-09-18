using UnityEngine;
using UnityEngine.Pool;

namespace PixelatedDigging.FX
{
    public class VoxelGridDigFXHandler : MonoBehaviour
    {
        [SerializeField] VoxelDigFX digFXPrefab;
        [SerializeField] int initialPoolSize;
        [SerializeField] int maxPoolSize;

        Vector3 effectScale;
        Material material;
        float textureVoxelResolution;

        ObjectPool<VoxelDigFX> fxPool;

        public void Initialize(float effectSize, float effectHeight, Material material,
            float textureVoxelResolution)
        {
            effectScale = new Vector3(effectSize, effectSize, effectHeight);
            this.material = material;
            this.textureVoxelResolution = textureVoxelResolution;

            fxPool = new ObjectPool<VoxelDigFX>(CreateEffect, OnGetEffect, OnReleaseEffect,
                DestroyEffect, false, initialPoolSize, maxPoolSize);
        }

        public void ShowEffect(Vector3 worldPosition, Vector2 gridMin, Vector2 gridMax,
            Vector2Int gridResolution)
        {
            var digEffect = fxPool.Get();
            digEffect.SetPositionAndRotation(worldPosition, Quaternion.identity);
            digEffect.Initialize(effectScale, material, textureVoxelResolution, gridMin, gridMax,
                gridResolution, fxPool.Release);
        }

        VoxelDigFX CreateEffect()
        {
            var effect = Instantiate(digFXPrefab, transform);
            effect.SetActive(false);
            return effect;
        }

        void DestroyEffect(VoxelDigFX effect)
        {
            Destroy(effect.gameObject);
        }

        void OnGetEffect(VoxelDigFX effect)
        {
            effect.SetActive(true);
        }

        void OnReleaseEffect(VoxelDigFX effect)
        {
            effect.SetActive(false);
        }
    }
}