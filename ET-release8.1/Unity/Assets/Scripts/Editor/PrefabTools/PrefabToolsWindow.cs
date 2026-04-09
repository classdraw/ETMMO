using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

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
            }

            EditorGUILayout.Space(6);
            EditorGUILayout.LabelField("说明：会打开预制体内容、递归清理后写回磁盘。", EditorStyles.wordWrappedMiniLabel);
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
    }
}
