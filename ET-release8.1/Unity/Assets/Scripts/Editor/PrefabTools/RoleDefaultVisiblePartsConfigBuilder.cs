using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace ET
{
    public static class RoleDefaultVisiblePartsConfigBuilder
    {
        public static RoleDefaultVisiblePartsConfig CreateOrUpdateFromModularCharacter01()
        {
            GameObject source = AssetDatabase.LoadAssetAtPath<GameObject>(SyntyModularCharacterDismantleTool.SourcePrefabPath);
            return CreateOrUpdateFromModularCharacter(source);
        }

        public static RoleDefaultVisiblePartsConfig CreateOrUpdateFromModularCharacter(GameObject sourcePrefabAsset)
        {
            if (!SyntyModularCharacterDismantleTool.TryResolveSourcePrefab(sourcePrefabAsset, out GameObject source, out _))
            {
                return null;
            }

            UnityEngine.GameObject instance = PrefabUtility.InstantiatePrefab(source) as GameObject;
            instance.hideFlags = HideFlags.HideAndDontSave;

            string[] femaleParts = CollectDefaultVisiblePartNames(instance, true);
            string[] maleParts = CollectDefaultVisiblePartNames(instance, false);
            UnityEngine.Object.DestroyImmediate(instance);

            RoleDefaultVisiblePartsConfig config = RoleDefaultVisiblePartsConfigImporter.LoadOrCreateConfig();
            if (config == null)
            {
                return null;
            }

            config.entries.Clear();
            string[] guids = AssetDatabase.FindAssets("t:Prefab", new[] { SyntyModularCharacterDismantleTool.RoleFolder });
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                string fileName = Path.GetFileNameWithoutExtension(path);
                if (!fileName.StartsWith("Role_"))
                {
                    continue;
                }

                bool isFemale = fileName.EndsWith("_F");
                bool isMale = fileName.EndsWith("_M");
                if (!isFemale && !isMale)
                {
                    continue;
                }

                config.entries.Add(new RoleDefaultVisiblePartsEntry
                {
                    rolePrefabName = fileName,
                    visiblePartNames = isFemale ? femaleParts : maleParts,
                });
            }

            EditorUtility.SetDirty(config);
            AssetDatabase.SaveAssets();
            Debug.Log(
                $"[Synty] 已写入 {RoleDefaultVisiblePartsConfig.DefaultAssetPath}，共 {config.entries.Count} 条；" +
                $"女默认部件 {femaleParts.Length} 个，男 {maleParts.Length} 个。");
            return config;
        }

        /// <summary>
        /// 仅扫描 Key 对应性别的 Female_Parts / Male_Parts，Chr 须 activeSelf 且 Renderer 启用；All_Gender 在根 activeSelf 时追加。
        /// </summary>
        public static string[] CollectDefaultVisiblePartNames(GameObject characterInstance, bool isFemale)
        {
            Transform modular = FindModularCharactersRoot(characterInstance.transform);
            if (modular == null)
            {
                return System.Array.Empty<string>();
            }

            var names = new List<string>();
            string genderParts = isFemale ? "Female_Parts" : "Male_Parts";
            Transform genderRoot = SyntyModularRolePartUtil.FindGenderPartsRoot(modular, genderParts);
            Transform allGender = SyntyModularRolePartUtil.FindGenderPartsRoot(modular, "All_Gender_Parts");

            AppendVisibleRendererNames(names, genderRoot, useActiveSelf: true, roleIsFemale: isFemale);
            if (allGender != null && allGender.gameObject.activeSelf)
            {
                AppendVisibleRendererNames(names, allGender, useActiveSelf: true, roleIsFemale: null);
            }

            return names.ToArray();
        }

        /// <summary>
        /// 读取预制体上 activeSelf 的 Female_Parts / Male_Parts / All_Gender_Parts（各根节点亦须 activeSelf）。
        /// </summary>
        public static string[] CollectVisiblePartNamesFromGenderFolders(GameObject characterInstance)
        {
            Transform modular = FindModularCharactersRoot(characterInstance.transform);
            if (modular == null)
            {
                return System.Array.Empty<string>();
            }

            var names = new List<string>();
            Transform femaleRoot = SyntyModularRolePartUtil.FindGenderPartsRoot(modular, "Female_Parts");
            Transform maleRoot = SyntyModularRolePartUtil.FindGenderPartsRoot(modular, "Male_Parts");
            Transform allGender = SyntyModularRolePartUtil.FindGenderPartsRoot(modular, "All_Gender_Parts");

            if (femaleRoot != null && femaleRoot.gameObject.activeSelf)
            {
                AppendVisibleRendererNames(names, femaleRoot, useActiveSelf: true, roleIsFemale: true);
            }

            if (maleRoot != null && maleRoot.gameObject.activeSelf)
            {
                AppendVisibleRendererNames(names, maleRoot, useActiveSelf: true, roleIsFemale: false);
            }

            if (allGender != null && allGender.gameObject.activeSelf)
            {
                AppendVisibleRendererNames(names, allGender, useActiveSelf: true, roleIsFemale: null);
            }

            return names.ToArray();
        }

        public static string[] CollectVisiblePartNamesFromSourcePrefab(GameObject sourcePrefabAsset)
        {
            return CollectVisiblePartNamesFromSourcePrefab(sourcePrefabAsset, null);
        }

        /// <param name="rolePrefabNameForGender">若非空且以 _F/_M 结尾，只收集对应性别 + All_Gender_Parts。</param>
        public static string[] CollectVisiblePartNamesFromSourcePrefab(GameObject sourcePrefabAsset, string rolePrefabNameForGender)
        {
            if (!SyntyModularCharacterDismantleTool.TryResolveSourcePrefab(
                    sourcePrefabAsset,
                    out GameObject source,
                    out string assetPath))
            {
                return System.Array.Empty<string>();
            }

            GameObject instance = PrefabUtility.InstantiatePrefab(source) as GameObject;
            instance.hideFlags = HideFlags.HideAndDontSave;
            try
            {
                if (SyntyModularRolePartUtil.TryParseRolePrefabGender(rolePrefabNameForGender, out bool isFemale, out _))
                {
                    string[] parts = CollectDefaultVisiblePartNames(instance, isFemale);
                    if (isFemale && CountGenderBodyParts(parts) < 8)
                    {
                        string malePrefabPath = SyntyModularRolePartUtil.TryGetPairedGenderPrefabPath(assetPath, wantMale: true);
                        if (!string.IsNullOrEmpty(malePrefabPath) && malePrefabPath != assetPath)
                        {
                            GameObject maleSource = AssetDatabase.LoadAssetAtPath<GameObject>(malePrefabPath);
                            GameObject maleInstance = PrefabUtility.InstantiatePrefab(maleSource) as GameObject;
                            if (maleInstance != null)
                            {
                                maleInstance.hideFlags = HideFlags.HideAndDontSave;
                                try
                                {
                                    MirrorActivePartSelection(maleInstance, instance, targetIsFemale: true);
                                    parts = CollectDefaultVisiblePartNames(instance, isFemale: true);
                                    Debug.Log(
                                        $"[Synty] 女 Key 身体件不足，已从配对男模 {malePrefabPath} 镜像勾选 → 共 {parts.Length} 项。");
                                }
                                finally
                                {
                                    UnityEngine.Object.DestroyImmediate(maleInstance);
                                }
                            }
                        }
                    }

                    return parts;
                }

                return CollectVisiblePartNamesFromGenderFolders(instance);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(instance);
            }
        }

        static int CountGenderBodyParts(string[] partNames)
        {
            if (partNames == null)
            {
                return 0;
            }

            int count = 0;
            foreach (string name in partNames)
            {
                if (SyntyModularRolePartUtil.HasFemaleGenderToken(name) ||
                    SyntyModularRolePartUtil.HasMaleGenderToken(name))
                {
                    count++;
                }
            }

            return count;
        }

        /// <summary>
        /// 将 donor 上 activeSelf 的 Chr（含 All_Gender）同步到 target 同名/异性名节点（仅内存，不写盘）。
        /// </summary>
        public static void MirrorActivePartSelection(GameObject donorInstance, GameObject targetInstance, bool targetIsFemale)
        {
            Transform donorModular = FindModularCharactersRoot(donorInstance.transform);
            Transform targetModular = FindModularCharactersRoot(targetInstance.transform);
            if (donorModular == null || targetModular == null)
            {
                return;
            }

            bool donorIsFemale = !targetIsFemale;
            Transform donorGender = SyntyModularRolePartUtil.FindGenderPartsRoot(
                donorModular,
                donorIsFemale ? "Female_Parts" : "Male_Parts");
            Transform targetGender = SyntyModularRolePartUtil.FindGenderPartsRoot(
                targetModular,
                targetIsFemale ? "Female_Parts" : "Male_Parts");
            Transform donorAll = SyntyModularRolePartUtil.FindGenderPartsRoot(donorModular, "All_Gender_Parts");
            Transform targetAll = SyntyModularRolePartUtil.FindGenderPartsRoot(targetModular, "All_Gender_Parts");

            if (donorGender != null)
            {
                donorGender.gameObject.SetActive(true);
            }

            if (targetGender != null)
            {
                targetGender.gameObject.SetActive(true);
            }

            if (donorAll != null)
            {
                donorAll.gameObject.SetActive(true);
            }

            if (targetAll != null)
            {
                targetAll.gameObject.SetActive(true);
            }

            MirrorActiveChrBetweenRoots(donorGender, targetGender, targetIsFemale, targetModular);
            MirrorActiveChrBetweenRoots(donorAll, targetAll, mapToFemale: null, targetModular);
        }

        static void MirrorActiveChrBetweenRoots(
            Transform fromRoot,
            Transform toRoot,
            bool? mapToFemale,
            Transform targetModular)
        {
            if (fromRoot == null || toRoot == null)
            {
                return;
            }

            foreach (Renderer fromRenderer in fromRoot.GetComponentsInChildren<Renderer>(true))
            {
                if (fromRenderer == null || fromRenderer.transform == fromRoot)
                {
                    continue;
                }

                if (!fromRenderer.gameObject.activeSelf || !fromRenderer.enabled)
                {
                    continue;
                }

                string targetChrName = fromRenderer.gameObject.name;
                if (mapToFemale.HasValue)
                {
                    targetChrName = SyntyModularRolePartUtil.MapGenderInName(targetChrName, mapToFemale.Value);
                }

                Transform toChr = FindDeepChildByName(toRoot, targetChrName);
                if (toChr == null)
                {
                    continue;
                }

                ActivateAncestorsForNode(toChr, targetModular);
                toChr.gameObject.SetActive(true);
                Renderer toRenderer = toChr.GetComponent<Renderer>();
                if (toRenderer != null)
                {
                    toRenderer.enabled = true;
                }
            }
        }

        static void ActivateAncestorsForActiveSelfSelections(Transform partsRoot, Transform modularRoot)
        {
            if (partsRoot == null)
            {
                return;
            }

            partsRoot.gameObject.SetActive(true);
            foreach (Renderer r in partsRoot.GetComponentsInChildren<Renderer>(true))
            {
                if (r == null || r.transform == partsRoot || !r.gameObject.activeSelf || !r.enabled)
                {
                    continue;
                }

                ActivateAncestorsForNode(r.transform, modularRoot);
            }
        }

        static void ActivateAncestorsForNode(Transform node, Transform modularRoot)
        {
            for (Transform walk = node; walk != null && walk != modularRoot.parent; walk = walk.parent)
            {
                walk.gameObject.SetActive(true);
            }
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

        static void AppendVisibleRendererNames(
            List<string> names,
            Transform partsRoot,
            bool useActiveSelf = true,
            bool? roleIsFemale = null)
        {
            if (partsRoot == null)
            {
                return;
            }

            Renderer[] renderers = partsRoot.GetComponentsInChildren<Renderer>(true);
            foreach (Renderer r in renderers)
            {
                if (r == null || r.transform == partsRoot)
                {
                    continue;
                }

                GameObject go = r.gameObject;
                if (!go.activeSelf || !r.enabled)
                {
                    continue;
                }

                if (roleIsFemale.HasValue &&
                    !SyntyModularRolePartUtil.PartNameMatchesRoleGender(go.name, roleIsFemale.Value))
                {
                    continue;
                }

                if (!names.Contains(go.name))
                {
                    names.Add(go.name);
                }
            }
        }

        static Transform FindModularCharactersRoot(Transform characterRoot)
        {
            Transform direct = FindDirectChild(characterRoot, "Modular_Characters");
            if (direct != null)
            {
                return direct;
            }

            foreach (Transform t in characterRoot.GetComponentsInChildren<Transform>(true))
            {
                if (t.name == "Modular_Characters")
                {
                    return t;
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
