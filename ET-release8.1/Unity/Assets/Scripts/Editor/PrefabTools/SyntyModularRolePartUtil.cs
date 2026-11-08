using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace ET
{
    /// <summary>
    /// Role 性别与 RolePart 路径（Female_* / Male_* / All_*）对齐。
    /// </summary>
    public static class SyntyModularRolePartUtil
    {
        public static bool TryParseRolePrefabGender(string rolePrefabName, out bool isFemale, out bool isMale)
        {
            isFemale = false;
            isMale = false;
            rolePrefabName = rolePrefabName?.Trim();
            if (string.IsNullOrEmpty(rolePrefabName))
            {
                return false;
            }

            isFemale = rolePrefabName.EndsWith("_F", System.StringComparison.Ordinal);
            isMale = rolePrefabName.EndsWith("_M", System.StringComparison.Ordinal);
            return isFemale || isMale;
        }

        public static bool IsGenderSpecificPartName(string partName)
        {
            if (string.IsNullOrEmpty(partName))
            {
                return false;
            }

            return partName.Contains("_Female_", System.StringComparison.Ordinal) ||
                   partName.Contains("_Male_", System.StringComparison.Ordinal) ||
                   partName.EndsWith("_Female", System.StringComparison.Ordinal) ||
                   partName.EndsWith("_Male", System.StringComparison.Ordinal);
        }

        public static bool PartNameMatchesRoleGender(string partName, bool roleIsFemale)
        {
            if (string.IsNullOrEmpty(partName))
            {
                return false;
            }

            bool hasFemale = HasFemaleGenderToken(partName);
            bool hasMale = HasMaleGenderToken(partName);
            if (hasFemale && hasMale)
            {
                return false;
            }

            if (!hasFemale && !hasMale)
            {
                return true;
            }

            return roleIsFemale ? hasFemale : hasMale;
        }

        /// <summary>避免 "_Female" 被 Contains("_Male") 误匹配。</summary>
        public static bool HasMaleGenderToken(string partName)
        {
            return Regex.IsMatch(partName, @"(^|_)Male(_|$)", RegexOptions.CultureInvariant);
        }

        public static bool HasFemaleGenderToken(string partName)
        {
            return Regex.IsMatch(partName, @"(^|_)Female(_|$)", RegexOptions.CultureInvariant);
        }

        public static string MapGenderInName(string name, bool toFemale)
        {
            if (string.IsNullOrEmpty(name))
            {
                return name;
            }

            if (toFemale)
            {
                return name.Replace("_Male_", "_Female_").Replace("Male_", "Female_");
            }

            return name.Replace("_Female_", "_Male_").Replace("Female_", "Male_");
        }

        public static string TryGetPairedGenderPrefabPath(string assetPath, bool wantMale)
        {
            if (string.IsNullOrEmpty(assetPath))
            {
                return null;
            }

            string dir = Path.GetDirectoryName(assetPath)?.Replace('\\', '/');
            string fileName = Path.GetFileNameWithoutExtension(assetPath);
            string ext = Path.GetExtension(assetPath);
            if (string.IsNullOrEmpty(dir) || string.IsNullOrEmpty(fileName))
            {
                return null;
            }

            if (!TryParseModularPrefabGenderSuffix(fileName, out string stem, out bool fileIsFemale))
            {
                return null;
            }

            bool fileIsMale = !fileIsFemale;
            if ((wantMale && fileIsMale) || (!wantMale && fileIsFemale))
            {
                return assetPath;
            }

            string suffix = wantMale ? "M" : "F";
            if (fileName.EndsWith("_F", System.StringComparison.Ordinal) ||
                fileName.EndsWith("_M", System.StringComparison.Ordinal))
            {
                suffix = wantMale ? "_M" : "_F";
            }

            string candidate = $"{dir}/{stem}{suffix}{ext}";
            return AssetDatabase.LoadAssetAtPath<GameObject>(candidate) != null ? candidate : null;
        }

        static bool TryParseModularPrefabGenderSuffix(string fileName, out string stem, out bool isFemale)
        {
            stem = null;
            isFemale = false;
            if (string.IsNullOrEmpty(fileName))
            {
                return false;
            }

            if (fileName.EndsWith("_F", System.StringComparison.Ordinal))
            {
                stem = fileName.Substring(0, fileName.Length - 2);
                isFemale = true;
                return true;
            }

            if (fileName.EndsWith("_M", System.StringComparison.Ordinal))
            {
                stem = fileName.Substring(0, fileName.Length - 2);
                isFemale = false;
                return true;
            }

            if (fileName.EndsWith("F", System.StringComparison.Ordinal))
            {
                stem = fileName.Substring(0, fileName.Length - 1);
                isFemale = true;
                return true;
            }

            if (fileName.EndsWith("M", System.StringComparison.Ordinal))
            {
                stem = fileName.Substring(0, fileName.Length - 1);
                isFemale = false;
                return true;
            }

            return false;
        }

        public static bool RolePartAssetFolderMatchesGender(string rolePartPrefabAssetPath, bool roleIsFemale)
        {
            if (string.IsNullOrEmpty(rolePartPrefabAssetPath))
            {
                return false;
            }

            string folder = Path.GetFileName(Path.GetDirectoryName(rolePartPrefabAssetPath)) ?? string.Empty;
            if (folder.StartsWith("Female_", System.StringComparison.Ordinal))
            {
                return roleIsFemale;
            }

            if (folder.StartsWith("Male_", System.StringComparison.Ordinal))
            {
                return !roleIsFemale;
            }

            return true;
        }

        public static string[] FilterPartsForRoleGender(IEnumerable<string> partNames, bool roleIsFemale)
        {
            var list = new List<string>();
            if (partNames == null)
            {
                return list.ToArray();
            }

            foreach (string partName in partNames)
            {
                if (string.IsNullOrWhiteSpace(partName))
                {
                    continue;
                }

                string trimmed = partName.Trim();
                if (!PartNameMatchesRoleGender(trimmed, roleIsFemale))
                {
                    continue;
                }

                if (!list.Contains(trimmed))
                {
                    list.Add(trimmed);
                }
            }

            return list.ToArray();
        }

        /// <summary>
        /// Synty 眉部件在 Role 预制体里常为 Chr_Female_Eyebrow_xx，拆解导出为 Chr_Eyebrow_Female_xx。
        /// </summary>
        /// <summary>将模型节点名转为 RolePart 预制体文件名（以 Bundles/Unit/RolePart 内实际资源为准）。</summary>
        public static string NormalizePartNameToRolePartPrefab(
            string partName,
            bool roleIsFemale,
            string rolePartRoot = SyntyModularCharacterDismantleTool.RolePartRoot)
        {
            partName = partName?.Trim();
            if (string.IsNullOrEmpty(partName))
            {
                return partName;
            }

            string path = ResolveRolePartPrefabPath(partName, roleIsFemale, null, rolePartRoot);
            if (!string.IsNullOrEmpty(path))
            {
                return Path.GetFileNameWithoutExtension(path);
            }

            return partName;
        }

        public static string[] NormalizePartNamesToRolePartPrefabs(IEnumerable<string> partNames, bool roleIsFemale)
        {
            var list = new List<string>();
            if (partNames == null)
            {
                return list.ToArray();
            }

            foreach (string partName in partNames)
            {
                string normalized = NormalizePartNameToRolePartPrefab(partName, roleIsFemale);
                if (string.IsNullOrEmpty(normalized) || list.Contains(normalized))
                {
                    continue;
                }

                list.Add(normalized);
            }

            return list.ToArray();
        }

        public static IEnumerable<string> EnumerateRolePartNameLookupCandidates(string partName)
        {
            if (string.IsNullOrEmpty(partName))
            {
                yield break;
            }

            yield return partName;

            var femaleEyebrow = Regex.Match(partName, @"^Chr_Female_Eyebrow_(\d+)$", RegexOptions.CultureInvariant);
            if (femaleEyebrow.Success)
            {
                yield return $"Chr_Eyebrow_Female_{femaleEyebrow.Groups[1].Value}";
            }

            var maleEyebrow = Regex.Match(partName, @"^Chr_Male_Eyebrow_(\d+)$", RegexOptions.CultureInvariant);
            if (maleEyebrow.Success)
            {
                yield return $"Chr_Eyebrow_Male_{maleEyebrow.Groups[1].Value}";
            }

            var eyebrowFemale = Regex.Match(partName, @"^Chr_Eyebrow_Female_(\d+)$", RegexOptions.CultureInvariant);
            if (eyebrowFemale.Success)
            {
                yield return $"Chr_Female_Eyebrow_{eyebrowFemale.Groups[1].Value}";
            }

            var eyebrowMale = Regex.Match(partName, @"^Chr_Eyebrow_Male_(\d+)$", RegexOptions.CultureInvariant);
            if (eyebrowMale.Success)
            {
                yield return $"Chr_Male_Eyebrow_{eyebrowMale.Groups[1].Value}";
            }
        }

        public static string ResolveRolePartPrefabPath(
            string partName,
            bool roleIsFemale,
            IReadOnlyDictionary<string, string> pathByPartName,
            string rolePartRoot)
        {
            if (string.IsNullOrEmpty(partName))
            {
                return null;
            }

            string genderMismatchFallback = null;
            foreach (string candidate in EnumerateRolePartNameLookupCandidates(partName))
            {
                if (pathByPartName != null &&
                    pathByPartName.TryGetValue(candidate, out string indexed) &&
                    RolePartAssetFolderMatchesGender(indexed, roleIsFemale))
                {
                    return indexed;
                }

                string suffix = $"/{candidate}.prefab";
                string[] guids = UnityEditor.AssetDatabase.FindAssets($"{candidate} t:Prefab", new[] { rolePartRoot });
                foreach (string guid in guids)
                {
                    string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
                    if (!path.EndsWith(suffix, System.StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    if (RolePartAssetFolderMatchesGender(path, roleIsFemale))
                    {
                        return path;
                    }

                    genderMismatchFallback ??= path;
                }
            }

            return genderMismatchFallback;
        }

        public static Transform FindGenderPartsRoot(Transform modular, string nodeName)
        {
            Transform direct = FindDirectChild(modular, nodeName);
            if (direct != null)
            {
                return direct;
            }

            foreach (Transform t in modular.GetComponentsInChildren<Transform>(true))
            {
                if (t != modular && t.name == nodeName)
                {
                    return t;
                }
            }

            return null;
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
