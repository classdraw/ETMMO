using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace ET
{
    public enum SyntyModularGenderOption
    {
        Female = 0,
        Male = 1,
    }

    public static class SyntyModularGenderSyncTool
    {
        public static bool ApplyGenderToPrefab(GameObject prefabAsset)
        {
            if (prefabAsset == null)
            {
                return false;
            }

            if (!SyntyModularRolePartUtil.TryParseRolePrefabGender(prefabAsset.name, out bool isFemale, out bool isMale))
            {
                EditorUtility.DisplayDialog(
                    "Synty模块化处理",
                    $"预制体「{prefabAsset.name}」名称须以 _F（女）或 _M（男）结尾。",
                    "确定");
                return false;
            }

            var gender = isFemale ? SyntyModularGenderOption.Female : SyntyModularGenderOption.Male;
            return ApplyGenderToPrefab(prefabAsset, gender);
        }

        public static bool ApplyGenderToPrefab(GameObject prefabAsset, SyntyModularGenderOption gender)
        {
            if (!SyntyModularCharacterDismantleTool.TryResolveSourcePrefab(prefabAsset, out GameObject _, out string assetPath))
            {
                return false;
            }

            GameObject root = PrefabUtility.LoadPrefabContents(assetPath);
            try
            {
                Transform modular = FindByPath(root.transform, "Modular_Characters");
                if (modular == null)
                {
                    EditorUtility.DisplayDialog("Synty模块化处理", "预制体缺少 Modular_Characters。", "确定");
                    return false;
                }

                Transform femaleParts = FindGenderPartsRoot(modular, "Female_Parts");
                Transform maleParts = FindGenderPartsRoot(modular, "Male_Parts");
                if (femaleParts == null || maleParts == null)
                {
                    EditorUtility.DisplayDialog("Synty模块化处理", "预制体缺少 Female_Parts 或 Male_Parts。", "确定");
                    return false;
                }

                bool toFemale = gender == SyntyModularGenderOption.Female;
                if (toFemale)
                {
                    bool maleWasActive = maleParts.gameObject.activeSelf;
                    maleParts.gameObject.SetActive(true);
                    SyncVisiblePartsBetweenGender(maleParts, femaleParts, toFemale: true);
                    ActivateAncestors(femaleParts, root.transform);
                    femaleParts.gameObject.SetActive(true);
                    maleParts.gameObject.SetActive(false);
                    if (!maleWasActive)
                    {
                        DeactivateAllRenderersUnder(maleParts);
                    }
                }
                else
                {
                    bool femaleWasActive = femaleParts.gameObject.activeSelf;
                    femaleParts.gameObject.SetActive(true);
                    SyncVisiblePartsBetweenGender(femaleParts, maleParts, toFemale: false);
                    ActivateAncestors(maleParts, root.transform);
                    maleParts.gameObject.SetActive(true);
                    femaleParts.gameObject.SetActive(false);
                    if (!femaleWasActive)
                    {
                        DeactivateAllRenderersUnder(femaleParts);
                    }
                }

                Transform allGender = FindDirectChild(modular, "All_Gender_Parts");
                if (allGender != null)
                {
                    allGender.gameObject.SetActive(true);
                }

                PrefabUtility.SaveAsPrefabAsset(root, assetPath);
                Debug.Log($"[Synty] 已应用性别 {gender} → {assetPath}");
                return true;
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        /// <summary>
        /// 将源性别侧当前显示的 Chr（Renderer 启用且 active）同步到目标性别对应槽位/同名规则部件。
        /// </summary>
        static void SyncVisiblePartsBetweenGender(Transform fromPartsRoot, Transform toPartsRoot, bool toFemale)
        {
            Dictionary<string, Transform> toSlotByName = BuildSlotFolderMap(toPartsRoot);
            HashSet<Transform> touchedToSlots = new HashSet<Transform>();

            foreach (Renderer fromRenderer in fromPartsRoot.GetComponentsInChildren<Renderer>(true))
            {
                if (fromRenderer == null || fromRenderer.transform == fromPartsRoot)
                {
                    continue;
                }

                GameObject fromGo = fromRenderer.gameObject;
                if (!fromGo.activeSelf || !fromRenderer.enabled)
                {
                    continue;
                }

                Transform fromSlot = FindParentSlotFolder(fromGo.transform, fromPartsRoot);
                if (fromSlot == null)
                {
                    continue;
                }

                string targetSlotName = MapGenderInName(fromSlot.name, toFemale);
                if (!toSlotByName.TryGetValue(targetSlotName, out Transform toSlot))
                {
                    toSlot = FindDeepChildByName(toPartsRoot, targetSlotName);
                }

                if (toSlot == null)
                {
                    continue;
                }

                string targetChrName = MapGenderInName(fromGo.name, toFemale);
                Transform toChr = FindDirectChildByNameRecursive(toSlot, targetChrName);
                if (toChr == null)
                {
                    toChr = FindBestMatchingChr(toSlot, fromGo.name, toFemale);
                }

                if (toChr == null)
                {
                    continue;
                }

                if (!touchedToSlots.Contains(toSlot))
                {
                    DeactivateAllRenderersUnder(toSlot);
                    touchedToSlots.Add(toSlot);
                }

                ActivateAncestors(toChr, toPartsRoot.parent);
                toChr.gameObject.SetActive(true);
                Renderer toRenderer = toChr.GetComponent<Renderer>();
                if (toRenderer != null)
                {
                    toRenderer.enabled = true;
                }
            }
        }

        static void ActivateAncestors(Transform node, Transform stopBefore)
        {
            Transform walk = node;
            while (walk != null && walk != stopBefore)
            {
                walk.gameObject.SetActive(true);
                walk = walk.parent;
            }
        }

        static Transform FindGenderPartsRoot(Transform modular, string nodeName)
        {
            Transform direct = FindDirectChild(modular, nodeName);
            if (direct != null)
            {
                return direct;
            }

            return FindDeepChildByName(modular, nodeName);
        }

        static Transform FindDeepChildByName(Transform root, string childName)
        {
            if (root == null || string.IsNullOrEmpty(childName))
            {
                return null;
            }

            foreach (Transform t in root.GetComponentsInChildren<Transform>(true))
            {
                if (t != root && t.name == childName)
                {
                    return t;
                }
            }

            return null;
        }

        static Transform FindBestMatchingChr(Transform slot, string sourceChrName, bool toFemale)
        {
            string mapped = MapGenderInName(sourceChrName, toFemale);
            if (!string.IsNullOrEmpty(mapped))
            {
                Transform exact = FindDirectChildByNameRecursive(slot, mapped);
                if (exact != null)
                {
                    return exact;
                }
            }

            Match num = Regex.Match(sourceChrName, @"_(\d+)$");
            if (!num.Success)
            {
                return null;
            }

            string tail = num.Groups[1].Value;
            foreach (Renderer r in slot.GetComponentsInChildren<Renderer>(true))
            {
                if (r == null || r.transform == slot)
                {
                    continue;
                }

                if (r.gameObject.name.EndsWith("_" + tail, System.StringComparison.Ordinal))
                {
                    return r.transform;
                }
            }

            return null;
        }

        static void DeactivateAllRenderersUnder(Transform slot)
        {
            foreach (Renderer r in slot.GetComponentsInChildren<Renderer>(true))
            {
                if (r == null || r.transform == slot)
                {
                    continue;
                }

                r.gameObject.SetActive(false);
            }
        }

        static Dictionary<string, Transform> BuildSlotFolderMap(Transform partsRoot)
        {
            var map = new Dictionary<string, Transform>();
            CollectSlotFolders(partsRoot, partsRoot, map);
            return map;
        }

        static void CollectSlotFolders(Transform partsRoot, Transform current, Dictionary<string, Transform> map)
        {
            for (int i = 0; i < current.childCount; i++)
            {
                Transform child = current.GetChild(i);
                if (child == partsRoot)
                {
                    continue;
                }

                if (HasRendererInSubtree(child))
                {
                    if (!map.ContainsKey(child.name))
                    {
                        map.Add(child.name, child);
                    }
                }
                else
                {
                    CollectSlotFolders(partsRoot, child, map);
                }
            }
        }

        static bool HasRendererInSubtree(Transform node)
        {
            foreach (Renderer r in node.GetComponentsInChildren<Renderer>(true))
            {
                if (r != null && r.transform != node)
                {
                    return true;
                }
            }

            return false;
        }

        static Transform FindParentSlotFolder(Transform chr, Transform partsRoot)
        {
            Transform best = null;
            Transform walk = chr.parent;
            while (walk != null && walk != partsRoot)
            {
                if (IsChrSlotFolder(walk.name))
                {
                    best = walk;
                }

                walk = walk.parent;
            }

            if (best != null)
            {
                return best;
            }

            return chr.parent != null && chr.parent != partsRoot ? chr.parent : null;
        }

        static bool IsChrSlotFolder(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return false;
            }

            return name.StartsWith("Male_", System.StringComparison.Ordinal) ||
                   name.StartsWith("Female_", System.StringComparison.Ordinal);
        }

        static string MapGenderInName(string name, bool toFemale)
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

        static Transform FindDirectChildByNameRecursive(Transform parent, string childName)
        {
            for (int i = 0; i < parent.childCount; i++)
            {
                Transform child = parent.GetChild(i);
                if (child.name == childName)
                {
                    return child;
                }

                Transform nested = FindDirectChildByNameRecursive(child, childName);
                if (nested != null)
                {
                    return nested;
                }
            }

            return null;
        }

        static Transform FindByPath(Transform root, string path)
        {
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
