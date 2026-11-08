using UnityEditor;
using UnityEngine;

namespace ET
{
    public static class SyntyModularRoleSkinnedRebindMenu
    {
        [MenuItem("Tools/预制体处理/Role 全部挂载绑骨脚本并重绑", false, 52)]
        static void RebindAllRolesInBundlesFolder()
        {
            if (!EditorUtility.DisplayDialog(
                    "Synty",
                    $"将为 {SyntyModularCharacterDismantleTool.RoleFolder} 下全部 Role_*.prefab\n" +
                    "添加 SyntyModularCharacterSkinnedBinder 并重新绑定骨骼，是否继续？",
                    "继续",
                    "取消"))
            {
                return;
            }

            int count = 0;
            string[] guids = AssetDatabase.FindAssets("t:Prefab", new[] { SyntyModularCharacterDismantleTool.RoleFolder });
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                string fileName = System.IO.Path.GetFileNameWithoutExtension(path);
                if (!fileName.StartsWith("Role_"))
                {
                    continue;
                }

                if (RebindRolePrefabAsset(path))
                {
                    count++;
                }
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"[Synty] 已处理 {count} 个 Role 预制体（挂载 SyntyModularCharacterSkinnedBinder + 绑骨）。");
        }

        [MenuItem("Tools/预制体处理/Role 重新绑定 SkinnedMesh 骨骼", false, 53)]
        static void RebindSelectedRoles()
        {
            if (Selection.gameObjects == null || Selection.gameObjects.Length == 0)
            {
                EditorUtility.DisplayDialog("Synty", "请在 Hierarchy 或 Project 中选中 Role 预制体。", "确定");
                return;
            }

            int count = 0;
            foreach (GameObject go in Selection.gameObjects)
            {
                if (go == null)
                {
                    continue;
                }

                string path = AssetDatabase.GetAssetPath(go);
                if (!string.IsNullOrEmpty(path) && path.EndsWith(".prefab", System.StringComparison.OrdinalIgnoreCase))
                {
                    if (RebindRolePrefabAsset(path))
                    {
                        count++;
                    }

                    continue;
                }

                if (RebindRoleInstance(go))
                {
                    count++;
                }
            }

            Debug.Log($"[Synty] 已重新绑定 {count} 个 Role 的 SkinnedMeshRenderer。");
        }

        static bool RebindRoleInstance(GameObject roleRoot)
        {
            EnsureBinder(roleRoot);
            Transform boneRoot = SyntySkinnedBoneBinder.ResolveSkeletonBoneRoot(roleRoot.transform);
            if (boneRoot == null)
            {
                return false;
            }

            SyntySkinnedBoneBinder.RebindAllSkinnedParts(roleRoot.transform, boneRoot);
            return true;
        }

        static bool RebindRolePrefabAsset(string prefabPath)
        {
            GameObject root = PrefabUtility.LoadPrefabContents(prefabPath);
            try
            {
                if (!RebindRoleInstance(root))
                {
                    return false;
                }

                PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
                return true;
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        static void EnsureBinder(GameObject roleRoot)
        {
            if (roleRoot.GetComponent<SyntyModularCharacterSkinnedBinder>() == null)
            {
                roleRoot.AddComponent<SyntyModularCharacterSkinnedBinder>();
            }
        }
    }
}
