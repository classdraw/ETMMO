using UnityEngine;

namespace ET
{
    /// <summary>
    /// 与 Demo2（Synty.DynamicTest）一致：按路径查找子节点（支持未激活节点）。
    /// </summary>
    public static class SyntyHierarchyPaths
    {
        public static Transform FindByPath(Transform root, string path)
        {
            if (root == null || string.IsNullOrEmpty(path))
            {
                return null;
            }

            string[] parts = path.Split('/');
            Transform current = root;
            foreach (string part in parts)
            {
                current = FindDirectChild(current, part);
                if (current == null)
                {
                    return null;
                }
            }

            return current;
        }

        public static void EnsureActiveHierarchy(Transform node)
        {
            for (Transform t = node; t != null; t = t.parent)
            {
                if (!t.gameObject.activeSelf)
                {
                    t.gameObject.SetActive(true);
                }
            }
        }

        static Transform FindDirectChild(Transform parent, string childName)
        {
            if (parent == null)
            {
                return null;
            }

            for (int i = 0; i < parent.childCount; i++)
            {
                Transform child = parent.GetChild(i);
                if (child.name == childName)
                {
                    return child;
                }
            }

            return null;
        }
    }
}
