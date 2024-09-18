using System;
using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

namespace PixelatedDigging.FX
{
    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(MeshRenderer))]
    public class VoxelDigFX : MonoBehaviour
    {
        Mesh mesh;

        public void Initialize(Vector3 scale, Material material, float textureVoxelResolution,
            Vector2 gridMin, Vector2 gridMax, Vector2Int gridResolution,
            Action<VoxelDigFX> disposer)
        {
            transform.localScale = scale;

            mesh = GetComponent<MeshFilter>().mesh;
            GetComponent<MeshRenderer>().sharedMaterial = material;
            ApplyUVs(textureVoxelResolution, gridMin, gridMax, gridResolution);

            StartCoroutine(Animate(() => disposer?.Invoke(this)));
        }

        void OnDestroy()
        {
            Destroy(mesh);
        }

        public void SetActive(bool active)
        {
            gameObject.SetActive(active);
        }

        public void SetPositionAndRotation(Vector3 position, Quaternion rotation)
        {
            transform.SetPositionAndRotation(position, rotation);
        }

        void ApplyUVs(float textureVoxelResolution, Vector2 gridMin, Vector2 gridMax,
            Vector2Int gridResolution)
        {
            var uvs = new Vector2[mesh.vertices.Length];

            for (int i = 0; i < mesh.vertices.Length; i++)
            {
                var vertexWorld = transform.TransformPoint(mesh.vertices[i]);
                var percentX = Mathf.InverseLerp(gridMin.x, gridMax.x, vertexWorld.x) *
                    gridResolution.x / textureVoxelResolution;
                var percentY = Mathf.InverseLerp(gridMin.y, gridMax.y, vertexWorld.y) *
                    gridResolution.y / textureVoxelResolution;
                uvs[i] = new Vector2(percentX, percentY);
            }
            mesh.uv = uvs;
        }

        IEnumerator Animate(Action callback)
        {
            var transform = this.transform;

            var duration = Random.Range(0.7f, 0.8f);
            int animCount = 0;

            // animate Z position
            var startZ = transform.position.z;
            var targetZ = startZ - 0.5f;
            StartCoroutine(LerpCoroutine(duration, t =>
            {
                var position = transform.position;
                position.z = Mathf.Lerp(startZ, targetZ, t);
                transform.position = position;
            },
            () => animCount--));
            animCount++;

            // animate Y position
            var startY = transform.position.y;
            var targetY = startY - 2f;
            StartCoroutine(InSineCoroutine(duration, t =>
            {
                var position = transform.position;
                position.y = Mathf.Lerp(startY, targetY, t);
                transform.position = position;
            },
            () => animCount--));
            animCount++;

            // animate rotation
            var rotationDelay = Random.Range(0.1f, 0.2f);
            var startRotation = transform.rotation;
            var targetRotation = startRotation * Quaternion.Euler(-45, 0, 0);
            StartCoroutine(InSineCoroutine(duration - rotationDelay, t =>
            {
                transform.rotation = Quaternion.Lerp(startRotation, targetRotation, t);
            },
            () =>
            {
                animCount--;
            },
            rotationDelay));
            animCount++;

            // animate scale
            var scaleDelay = Random.Range(0.2f, 0.3f);
            var startScale = transform.localScale;
            StartCoroutine(InSineCoroutine(duration - scaleDelay, t =>
            {
                transform.localScale = Vector3.Lerp(startScale, Vector3.zero, t);
            },
            () =>
            {
                animCount--;
            },
            scaleDelay));
            animCount++;

            yield return new WaitWhile(() => animCount > 0);
            callback?.Invoke();
        }

        IEnumerator LerpCoroutine(float duration, Action<float> action, Action callback,
            float delay = 0)
        {
            if (delay > 0)
                yield return new WaitForSeconds(delay);

            float percent = 0f;

            while (percent < 1)
            {
                percent += Time.deltaTime / duration;
                action?.Invoke(Mathf.Min(percent, 1));
                yield return null;
            }
            callback?.Invoke();
        }

        IEnumerator InSineCoroutine(float duration, Action<float> action, Action callback,
            float delay = 0)
        {
            if (delay > 0)
                yield return new WaitForSeconds(delay);

            float percent = 0f;

            while (percent < 1)
            {
                percent += Time.deltaTime / duration;
                var a = Mathf.Lerp(0, 90, Mathf.Min(percent, 1));
                var t = 1 - Mathf.Cos(a * Mathf.Deg2Rad);
                action?.Invoke(t);
                yield return null;
            }
            callback?.Invoke();
        }
    }
}