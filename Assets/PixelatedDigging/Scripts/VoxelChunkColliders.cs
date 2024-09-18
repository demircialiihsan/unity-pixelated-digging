using System.Collections.Generic;
using UnityEngine;

namespace PixelatedDigging
{
    public class VoxelChunkColliders : MonoBehaviour
    {
        List<List<int>> colliderPaths;
        List<EdgeCollider2D> colliders;

        public void Initialize()
        {
            colliderPaths = new List<List<int>>();
            colliders = new List<EdgeCollider2D>();
        }

        public void Clear()
        {
            colliderPaths.Clear();

            foreach (var collider in colliders)
                Destroy(collider);
            colliders.Clear();
        }

        public void GenerateColliders(List<Vector3> vertices)
        {
            foreach (var path in colliderPaths)
            {
                var collider = gameObject.AddComponent<EdgeCollider2D>();
                colliders.Add(collider);

                var points = new Vector2[path.Count];
                for (int i = 0; i < path.Count; i++)
                    points[i] = vertices[path[i]];

                collider.points = points;
            }
        }

        public void AddColliderPaths(int cellType, int a, int b, int c, int d)
        {
            switch (cellType)
            {
                case 0:
                    AddColliderPath(new() { a, b, d, c, a });
                    break;

                case 1:
                    AddColliderPath(new() { b, d, c, a });
                    break;
                case 2:
                    AddColliderPath(new() { a, b, d, c });
                    break;
                case 4:
                    AddColliderPath(new() { c, a, b, d });
                    break;
                case 8:
                    AddColliderPath(new() { d, c, a, b });
                    break;

                case 3:
                    AddColliderPath(new() { b, d, c });
                    break;
                case 6:
                    AddColliderPath(new() { a, b, d });
                    break;
                case 12:
                    AddColliderPath(new() { c, a, b });
                    break;
                case 9:
                    AddColliderPath(new() { d, c, a });
                    break;
                case 5:
                    AddColliderPath(new() { c, a });
                    AddColliderPath(new() { b, d });
                    break;
                case 10:
                    AddColliderPath(new() { a, b });
                    AddColliderPath(new() { d, c });
                    break;

                case 7:
                    AddColliderPath(new() { b, d });
                    break;
                case 14:
                    AddColliderPath(new() { a, b });
                    break;
                case 13:
                    AddColliderPath(new() { c, a });
                    break;
                case 11:
                    AddColliderPath(new() { d, c });
                    break;

                default:
                    break;
            }
        }

        void AddColliderPath(List<int> newPath)
        {
            if (CanExtendExistingPath(newPath, out var compatiblePath, out var extendsFromBack))
            {
                if (extendsFromBack)
                {
                    newPath.RemoveAt(newPath.Count - 1);
                    compatiblePath.InsertRange(0, newPath);

                    // after extension check if can merge an existing path from the other end
                    if (CanMergeExistingPath(compatiblePath, extendsFromBack, out var pathToMerge))
                    {
                        pathToMerge.RemoveAt(pathToMerge.Count - 1);
                        compatiblePath.InsertRange(0, pathToMerge);

                        colliderPaths.Remove(pathToMerge);
                    }
                }
                else
                {
                    newPath.RemoveAt(0);
                    compatiblePath.AddRange(newPath);

                    // after extension check if can merge an existing path from the other end
                    if (CanMergeExistingPath(compatiblePath, extendsFromBack, out var pathToMerge))
                    {
                        pathToMerge.RemoveAt(0);
                        compatiblePath.AddRange(pathToMerge);

                        colliderPaths.Remove(pathToMerge);
                    }
                }
            }
            else
            {
                colliderPaths.Add(newPath);
            }
        }

        bool CanExtendExistingPath(List<int> newPath, out List<int> compatiblePath,
            out bool extendsFromBack)
        {
            foreach (var path in colliderPaths)
            {
                if (path[0] == path[^1]) // closed path
                    continue;

                if (newPath[0] == path[^1])
                {
                    compatiblePath = path;
                    extendsFromBack = false;
                    return true;
                }
                else if (newPath[^1] == path[0])
                {
                    compatiblePath = path;
                    extendsFromBack = true;
                    return true;
                }
            }
            compatiblePath = default;
            extendsFromBack = default;
            return false;
        }

        bool CanMergeExistingPath(List<int> extendedPath, bool mergeFromBack,
            out List<int> pathToMerge)
        {
            foreach (var path in colliderPaths)
            {
                if (path == extendedPath)
                    continue;

                if (path[0] == path[^1]) // closed path
                    continue;

                if (mergeFromBack)
                {
                    if (extendedPath[0] == path[^1])
                    {
                        pathToMerge = path;
                        return true;
                    }
                }
                else
                {
                    if (extendedPath[^1] == path[0])
                    {
                        pathToMerge = path;
                        return true;
                    }
                }
            }
            pathToMerge = default;
            return false;
        }
    }
}