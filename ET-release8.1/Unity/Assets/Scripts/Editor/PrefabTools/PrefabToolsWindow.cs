using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ET
{
    /// <summary>
    /// 预制体批量处理工具入口。
    /// </summary>
    public class PrefabToolsWindow : EditorWindow
    {
        [MenuItem("Tools/预制体处理工具合集", false, 50)]
        public static void Open()
        {
            var w = GetWindow<PrefabToolsWindow>(true, "预制体处理工具", true);
            w.minSize = new Vector2(380, 220);
        }

        private void OnGUI()
        {
            EditorGUILayout.Space(8);
            EditorGUILayout.HelpBox("在 Project 中选中一个或多个预制体资源（.prefab），再点击下方按钮执行对应处理。", MessageType.Info);
            EditorGUILayout.Space(6);

            int prefabCount = CountSelectedPrefabPaths();
            EditorGUILayout.LabelField("当前选中的 .prefab 数量", prefabCount.ToString());

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("功能", EditorStyles.boldLabel);

            using (new EditorGUI.DisabledScope(prefabCount == 0))
            {
                if (GUILayout.Button("移除 Missing 脚本（含子物体）", GUILayout.Height(34)))
                {
                    RemoveMissingScriptsFromSelectedPrefabs();
                }

                if (GUILayout.Button("根节点 RectTransform → Transform（仅根为 Rect 时）", GUILayout.Height(34)))
                {
                    ConvertPrefabRootRectTransformToTransform();
                }
            }

            EditorGUILayout.Space(6);
            EditorGUILayout.LabelField("说明：会打开预制体内容、递归清理后写回磁盘。", EditorStyles.wordWrappedMiniLabel);
            EditorGUILayout.LabelField(
                "Rect→Transform：新建普通 Transform 根节点，迁移子物体并复制根上除 Transform 外的组件；无法原地移除 RectTransform。",
                EditorStyles.wordWrappedMiniLabel);
        }

        private static int CountSelectedPrefabPaths()
        {
            return CollectSelectedPrefabPaths().Count;
        }

        private static void RemoveMissingScriptsFromSelectedPrefabs()
        {
            List<string> paths = CollectSelectedPrefabPaths();
            if (paths.Count == 0)
            {
                EditorUtility.DisplayDialog("预制体处理工具", "请先在 Project 中选中至少一个 .prefab 资源。", "确定");
                return;
            }

            if (!EditorUtility.DisplayDialog("预制体处理工具",
                    $"将对 {paths.Count} 个预制体移除所有 Missing Script（递归子物体）。\n是否继续？",
                    "确定", "取消"))
            {
                return;
            }

            int totalRemoved = 0;
            int modifiedPrefabs = 0;

            AssetDatabase.StartAssetEditing();
            try
            {
                foreach (string path in paths)
                {
                    GameObject root = PrefabUtility.LoadPrefabContents(path);
                    try
                    {
                        int n = RemoveMissingScriptsRecursive(root);
                        if (n > 0)
                        {
                            PrefabUtility.SaveAsPrefabAsset(root, path);
                            totalRemoved += n;
                            modifiedPrefabs++;
                        }
                    }
                    finally
                    {
                        PrefabUtility.UnloadPrefabContents(root);
                    }
                }
            }
            finally
            {
                AssetDatabase.StopAssetEditing();
            }

            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("预制体处理工具",
                $"已扫描 {paths.Count} 个预制体，其中 {modifiedPrefabs} 个有修改；共移除 {totalRemoved} 处 Missing 脚本。",
                "确定");
        }

        private static List<string> CollectSelectedPrefabPaths()
        {
            var set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (UnityEngine.Object o in Selection.objects)
            {
                if (o == null)
                {
                    continue;
                }

                string path = AssetDatabase.GetAssetPath(o);
                if (string.IsNullOrEmpty(path))
                {
                    continue;
                }

                if (!path.EndsWith(".prefab", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                set.Add(path);
            }

            return new List<string>(set);
        }

        private static int RemoveMissingScriptsRecursive(GameObject go)
        {
            int removed = GameObjectUtility.RemoveMonoBehavioursWithMissingScript(go);
            Transform t = go.transform;
            for (int i = 0; i < t.childCount; i++)
            {
                removed += RemoveMissingScriptsRecursive(t.GetChild(i).gameObject);
            }

            return removed;
        }

        /// <summary>
        /// Unity 无法在同一 GameObject 上把 RectTransform 改成普通 Transform。
        /// 做法：新建仅含 Transform 的根节点，迁移子物体，复制根上除 Transform 系外的组件，再写回预制体。
        /// </summary>
        private static void ConvertPrefabRootRectTransformToTransform()
        {
            List<string> paths = CollectSelectedPrefabPaths();
            if (paths.Count == 0)
            {
                EditorUtility.DisplayDialog("预制体处理工具", "请先在 Project 中选中至少一个 .prefab 资源。", "确定");
                return;
            }

            if (!EditorUtility.DisplayDialog("预制体处理工具",
                    $"将对 {paths.Count} 个预制体尝试把根节点从 RectTransform 换成普通 Transform（新建根并迁移子物体）。\n" +
                    "若根上挂有仅 UI 可用的脚本，可能复制失败并会在 Console 提示。\n是否继续？",
                    "确定", "取消"))
            {
                return;
            }

            int converted = 0;
            int skipped = 0;
            int failed = 0;

            AssetDatabase.StartAssetEditing();
            try
            {
                foreach (string path in paths)
                {
                    GameObject oldRoot = null;
                    try
                    {
                        oldRoot = PrefabUtility.LoadPrefabContents(path);
                        if (!(oldRoot.transform is RectTransform))
                        {
                            skipped++;
                            PrefabUtility.UnloadPrefabContents(oldRoot);
                            oldRoot = null;
                            continue;
                        }

                        GameObject newRoot = new GameObject(oldRoot.name);
                        newRoot.transform.SetPositionAndRotation(oldRoot.transform.position, oldRoot.transform.rotation);
                        newRoot.transform.localScale = oldRoot.transform.localScale;

                        while (oldRoot.transform.childCount > 0)
                        {
                            Transform child = oldRoot.transform.GetChild(0);
                            child.SetParent(newRoot.transform, true);
                        }

                        foreach (Component c in oldRoot.GetComponents<Component>())
                        {
                            if (c == null || c is Transform)
                            {
                                continue;
                            }

                            try
                            {
                                UnityEditorInternal.ComponentUtility.CopyComponent(c);
                                UnityEditorInternal.ComponentUtility.PasteComponentAsNew(newRoot);
                            }
                            catch (Exception e)
                            {
                                Debug.LogWarning($"[预制体工具] 复制组件失败: {path} -> {c.GetType().Name}: {e.Message}");
                            }
                        }

                        UnityEngine.Object.DestroyImmediate(oldRoot);
                        oldRoot = null;

                        PrefabUtility.SaveAsPrefabAsset(newRoot, path);
                        PrefabUtility.UnloadPrefabContents(newRoot);
                        converted++;
                    }
                    catch (Exception e)
                    {
                        failed++;
                        Debug.LogError($"[预制体工具] 处理失败: {path}\n{e}");
                        if (oldRoot != null)
                        {
                            try
                            {
                                PrefabUtility.UnloadPrefabContents(oldRoot);
                            }
                            catch
                            {
                                // ignore
                            }
                        }
                    }
                }
            }
            finally
            {
                AssetDatabase.StopAssetEditing();
            }

            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("预制体处理工具",
                $"处理完成。\n成功替换根节点：{converted}\n跳过（根非 RectTransform）：{skipped}\n失败：{failed}",
                "确定");
        }
    }
}
