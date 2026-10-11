using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace ET
{
    public static class SpineDataConfigBuilder
    {
        public const string OutputFolder = UnitPartSlotCollectionBuilder.OutputFolder;
        public const string OutputAssetPath = OutputFolder + "/SpineDataConfig.asset";

        private const string SpumMatchingListTypeName = "SPUM_MatchingList";

        public static bool TryBuildFromMatchingPrefab(GameObject prefabAsset, out string message)
        {
            message = string.Empty;
            if (prefabAsset == null)
            {
                return true;
            }

            string prefabPath = AssetDatabase.GetAssetPath(prefabAsset);
            if (string.IsNullOrEmpty(prefabPath))
            {
                message = "请拖入 Project 中的预制体资源，而不是场景实例。";
                return false;
            }

            Component matchingList = FindSpumMatchingListComponent(prefabAsset);
            if (matchingList == null)
            {
                message = "预制体上未找到 SPUM_MatchingList 组件。";
                return false;
            }

            FieldInfo tablesField = matchingList.GetType().GetField("matchingTables", BindingFlags.Instance | BindingFlags.Public);
            if (tablesField?.GetValue(matchingList) is not IList tables)
            {
                message = "SPUM_MatchingList.matchingTables 为空或无法读取。";
                return false;
            }

            var entries = new List<SpineDataConfigEntry>(tables.Count);
            FieldInfo indexField = null;
            for (int i = 0; i < tables.Count; i++)
            {
                object row = tables[i];
                if (row == null)
                {
                    continue;
                }

                indexField ??= row.GetType().GetField("Index", BindingFlags.Instance | BindingFlags.Public);
                int index = 0;
                if (indexField != null)
                {
                    object indexValue = indexField.GetValue(row);
                    if (indexValue is int intIndex)
                    {
                        index = intIndex;
                    }
                }

                entries.Add(new SpineDataConfigEntry
                {
                    slotPart = i,
                    index = index,
                });
            }

            EnsureOutputFolder();

            SpineDataConfig config = AssetDatabase.LoadAssetAtPath<SpineDataConfig>(OutputAssetPath);
            if (config == null)
            {
                config = ScriptableObject.CreateInstance<SpineDataConfig>();
                AssetDatabase.CreateAsset(config, OutputAssetPath);
            }

            config.sourcePrefabPath = prefabPath;
            config.entries = entries;

            EditorUtility.SetDirty(config);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            message = $"已生成 SpineDataConfig：{entries.Count} 条。\n{OutputAssetPath}";
            return true;
        }

        private static Component FindSpumMatchingListComponent(GameObject prefabRoot)
        {
            Component[] components = prefabRoot.GetComponentsInChildren<Component>(true);
            for (int i = 0; i < components.Length; i++)
            {
                Component component = components[i];
                if (component != null && component.GetType().Name == SpumMatchingListTypeName)
                {
                    return component;
                }
            }

            return null;
        }

        private static void EnsureOutputFolder()
        {
            if (!AssetDatabase.IsValidFolder("Assets/Bundles"))
            {
                AssetDatabase.CreateFolder("Assets", "Bundles");
            }

            if (!AssetDatabase.IsValidFolder(OutputFolder))
            {
                AssetDatabase.CreateFolder("Assets/Bundles", "ScriptObject");
            }
        }
    }
}
