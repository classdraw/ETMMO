using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace ET
{
    public static class RoleDefaultVisiblePartsConfigImporter
    {
        public static bool ImportEntry(string rolePrefabName, string commaSeparatedPartNames)
        {
            return ImportEntry(rolePrefabName, ParsePartNames(commaSeparatedPartNames));
        }

        /// <summary>拆解流程写入：无弹窗，仅记录警告。</summary>
        public static bool TryImportEntryForDismantle(string rolePrefabName, IList<string> partNames)
        {
            rolePrefabName = rolePrefabName?.Trim();
            if (string.IsNullOrEmpty(rolePrefabName))
            {
                return false;
            }

            string[] names = NormalizePartNames(partNames);
            if (names.Length == 0)
            {
                return false;
            }

            if (!SyntyModularRolePartUtil.TryParseRolePrefabGender(rolePrefabName, out bool isFemale, out _))
            {
                return false;
            }

            names = SyntyModularRolePartUtil.FilterPartsForRoleGender(names, isFemale);
            var kept = new List<string>(names.Length);
            foreach (string part in names)
            {
                string path = SyntyModularRolePartUtil.ResolveRolePartPrefabPath(
                    part,
                    isFemale,
                    null,
                    SyntyModularCharacterDismantleTool.RolePartRoot);
                if (string.IsNullOrEmpty(path))
                {
                    Debug.LogWarning($"[Synty拆解] 默认部件未找到 RolePart，已跳过：{part}");
                    continue;
                }

                kept.Add(SyntyModularRolePartUtil.NormalizePartNameToRolePartPrefab(part, isFemale));
            }

            if (kept.Count == 0)
            {
                return false;
            }

            string[] normalized = SyntyModularRolePartUtil.NormalizePartNamesToRolePartPrefabs(kept, isFemale);

            RoleDefaultVisiblePartsConfig config = LoadOrCreateConfig();
            if (config == null)
            {
                return false;
            }

            WriteEntryWithSerializedObject(config, rolePrefabName, normalized);
            Debug.Log(
                $"[Synty拆解] 已从源模型更新 {rolePrefabName} 默认部件 {normalized.Length} 项 → {RoleDefaultVisiblePartsConfig.DefaultAssetPath}");
            return true;
        }

        public static bool ImportEntry(string rolePrefabName, IList<string> partNames)
        {
            rolePrefabName = rolePrefabName?.Trim();
            if (string.IsNullOrEmpty(rolePrefabName))
            {
                EditorUtility.DisplayDialog("导入默认部件", "请填写 Role 预制体名称（Key）。", "确定");
                return false;
            }

            string[] names = NormalizePartNames(partNames);
            if (names.Length == 0)
            {
                EditorUtility.DisplayDialog("导入默认部件", "部件列表为空，请至少添加一个 Chr_* 名称。", "确定");
                return false;
            }

            if (SyntyModularRolePartUtil.TryParseRolePrefabGender(rolePrefabName, out bool isFemale, out _))
            {
                names = ValidateAndFilterPartsForRole(rolePrefabName, names, isFemale);
                if (names.Length == 0)
                {
                    return false;
                }
            }
            else
            {
                EditorUtility.DisplayDialog(
                    "导入默认部件",
                    "Key 须以 _F 或 _M 结尾（如 Role_Knight_01_F），才能与 Female_* / Male_* 部件路径对应。",
                    "确定");
                return false;
            }

            RoleDefaultVisiblePartsConfig config = LoadOrCreateConfig();
            if (config == null)
            {
                return false;
            }

            WriteEntryWithSerializedObject(config, rolePrefabName, names);
            Debug.Log($"[Synty] 已写入 {rolePrefabName} 默认部件 {names.Length} 个 → {RoleDefaultVisiblePartsConfig.DefaultAssetPath}");
            return true;
        }

        static void WriteEntryWithSerializedObject(RoleDefaultVisiblePartsConfig config, string rolePrefabName, string[] names)
        {
            Undo.RecordObject(config, "Save Role Default Visible Parts");

            SerializedObject serialized = new SerializedObject(config);
            SerializedProperty entriesProp = serialized.FindProperty("entries");
            if (entriesProp == null || !entriesProp.isArray)
            {
                return;
            }

            int targetIndex = -1;
            for (int i = 0; i < entriesProp.arraySize; i++)
            {
                SerializedProperty element = entriesProp.GetArrayElementAtIndex(i);
                SerializedProperty nameProp = element.FindPropertyRelative("rolePrefabName");
                if (nameProp != null && nameProp.stringValue == rolePrefabName)
                {
                    targetIndex = i;
                    break;
                }
            }

            if (targetIndex < 0)
            {
                targetIndex = entriesProp.arraySize;
                entriesProp.InsertArrayElementAtIndex(targetIndex);
            }

            SerializedProperty entry = entriesProp.GetArrayElementAtIndex(targetIndex);
            entry.FindPropertyRelative("rolePrefabName").stringValue = rolePrefabName;

            SerializedProperty partsProp = entry.FindPropertyRelative("visiblePartNames");
            partsProp.ClearArray();
            for (int i = 0; i < names.Length; i++)
            {
                partsProp.InsertArrayElementAtIndex(i);
                partsProp.GetArrayElementAtIndex(i).stringValue = names[i];
            }

            serialized.ApplyModifiedProperties();
            EditorUtility.SetDirty(config);
            AssetDatabase.SaveAssets();
        }

        public static string[] TryGetPartNames(string rolePrefabName)
        {
            rolePrefabName = rolePrefabName?.Trim();
            if (string.IsNullOrEmpty(rolePrefabName))
            {
                return null;
            }

            RoleDefaultVisiblePartsConfig config = TryLoadConfig();
            if (config == null)
            {
                return null;
            }

            string[] fromRuntime = config.GetVisiblePartNames(rolePrefabName);
            if (fromRuntime != null && fromRuntime.Length > 0)
            {
                return fromRuntime;
            }

            var serialized = new SerializedObject(config);
            SerializedProperty entriesProp = serialized.FindProperty("entries");
            if (entriesProp == null || !entriesProp.isArray)
            {
                return null;
            }

            for (int i = 0; i < entriesProp.arraySize; i++)
            {
                SerializedProperty element = entriesProp.GetArrayElementAtIndex(i);
                if (element.FindPropertyRelative("rolePrefabName").stringValue != rolePrefabName)
                {
                    continue;
                }

                SerializedProperty partsProp = element.FindPropertyRelative("visiblePartNames");
                if (partsProp == null || !partsProp.isArray || partsProp.arraySize == 0)
                {
                    return Array.Empty<string>();
                }

                var list = new List<string>(partsProp.arraySize);
                for (int p = 0; p < partsProp.arraySize; p++)
                {
                    string part = partsProp.GetArrayElementAtIndex(p).stringValue;
                    if (!string.IsNullOrEmpty(part))
                    {
                        list.Add(part);
                    }
                }

                return list.ToArray();
            }

            return null;
        }

        static string[] ValidateAndFilterPartsForRole(string rolePrefabName, string[] names, bool roleIsFemale)
        {
            var mismatched = new List<string>();
            var kept = new List<string>(names.Length);
            foreach (string name in names)
            {
                if (SyntyModularRolePartUtil.PartNameMatchesRoleGender(name, roleIsFemale))
                {
                    kept.Add(name);
                }
                else if (SyntyModularRolePartUtil.IsGenderSpecificPartName(name))
                {
                    mismatched.Add(name);
                }
                else
                {
                    kept.Add(name);
                }
            }

            if (mismatched.Count > 0)
            {
                string genderLabel = roleIsFemale ? "女（_F / Female_*）" : "男（_M / Male_*）";
                string sample = string.Join("\n", mismatched.GetRange(0, Mathf.Min(6, mismatched.Count)));
                if (mismatched.Count > 6)
                {
                    sample += $"\n… 共 {mismatched.Count} 项";
                }

                if (!EditorUtility.DisplayDialog(
                        "导入默认部件",
                        $"{rolePrefabName} 为{genderLabel}，下列部件性别不匹配，将不会写入：\n\n{sample}\n\n是否继续保存其余 {kept.Count} 项？",
                        "继续保存",
                        "取消"))
                {
                    return Array.Empty<string>();
                }
            }

            var missingPrefab = new List<string>();
            foreach (string part in kept)
            {
                string path = SyntyModularRolePartUtil.ResolveRolePartPrefabPath(
                    part,
                    roleIsFemale,
                    null,
                    SyntyModularCharacterDismantleTool.RolePartRoot);
                if (string.IsNullOrEmpty(path))
                {
                    missingPrefab.Add(part);
                }
            }

            if (missingPrefab.Count > 0)
            {
                string sample = string.Join("\n", missingPrefab.GetRange(0, Mathf.Min(6, missingPrefab.Count)));
                EditorUtility.DisplayDialog(
                    "导入默认部件",
                    $"下列部件在 RolePart 下未找到与性别匹配的预制体（Female_* / Male_*）：\n\n{sample}\n\n请从模型重新读取或修正名称后再保存。",
                    "确定");
                return Array.Empty<string>();
            }

            return SyntyModularRolePartUtil.NormalizePartNamesToRolePartPrefabs(kept, roleIsFemale);
        }

        static string[] NormalizePartNames(IList<string> partNames)
        {
            if (partNames == null || partNames.Count == 0)
            {
                return Array.Empty<string>();
            }

            var list = new List<string>(partNames.Count);
            foreach (string item in partNames)
            {
                string name = item?.Trim();
                if (string.IsNullOrEmpty(name) || list.Contains(name))
                {
                    continue;
                }

                list.Add(name);
            }

            return list.ToArray();
        }

        public static RoleDefaultVisiblePartsConfig LoadOrCreateConfig()
        {
            RoleDefaultVisiblePartsConfig config = TryLoadConfig();
            if (config != null)
            {
                return config;
            }

            string assetPath = RoleDefaultVisiblePartsConfig.DefaultAssetPath;
            string dir = Path.GetDirectoryName(assetPath)?.Replace('\\', '/');
            if (string.IsNullOrEmpty(dir) || !AssetDatabase.IsValidFolder(dir))
            {
                EditorUtility.DisplayDialog(
                    "导入默认部件",
                    $"目录不存在，请先创建资源或放到：\n{assetPath}",
                    "确定");
                return null;
            }

            config = ScriptableObject.CreateInstance<RoleDefaultVisiblePartsConfig>();
            AssetDatabase.CreateAsset(config, assetPath);
            AssetDatabase.SaveAssets();
            return config;
        }

        static RoleDefaultVisiblePartsConfig TryLoadConfig()
        {
            const string legacyPath = "Assets/Scripts/Editor/PrefabTools/RoleDefaultVisiblePartsConfig.asset";
            RoleDefaultVisiblePartsConfig config =
                AssetDatabase.LoadAssetAtPath<RoleDefaultVisiblePartsConfig>(
                    RoleDefaultVisiblePartsConfig.DefaultAssetPath);
            if (config != null)
            {
                return config;
            }

            return AssetDatabase.LoadAssetAtPath<RoleDefaultVisiblePartsConfig>(legacyPath);
        }

        public static string[] ParsePartNames(string commaSeparatedPartNames)
        {
            if (string.IsNullOrWhiteSpace(commaSeparatedPartNames))
            {
                return Array.Empty<string>();
            }

            string[] raw = commaSeparatedPartNames.Split(',');
            var list = new List<string>(raw.Length);
            foreach (string item in raw)
            {
                string name = item.Trim();
                if (string.IsNullOrEmpty(name))
                {
                    continue;
                }

                if (!list.Contains(name))
                {
                    list.Add(name);
                }
            }

            return list.ToArray();
        }
    }
}
