using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace ET
{
    /// <summary>
    /// 拆解 Synty ModularCharacter_01：部件 → RolePart；Role_*_F/_M 在 Body/Head 同级挂 Root（内为骨架空壳）。
    /// </summary>
    public static class SyntyModularCharacterDismantleTool
    {
        public const string SourcePrefabPath =
            "Assets/Art/Synty/PolygonFantasyHeroCharacters/Prefabs/FixedScale/ModularCharacter_01.prefab";

        public const string RoleFolder = "Assets/Bundles/Unit/Role";

        public const string RolePartRoot = "Assets/Bundles/Unit/RolePart";

        const string LegacySkeletonPath = "Assets/Bundles/Unit/Role/ModularCharacter_Skeleton.prefab";

        const string ModularCharactersFbxPath =
            "Assets/Art/Synty/PolygonFantasyHeroCharacters/Models/FixedScale/ModularCharacters.fbx";

        const string RoleAnimatorFolder = "Assets/Bundles/Animator/Role";

        const string ModularCharactersAvatarName = "ModularCharactersAvatar";

        public static void DismantleModularCharacter01()
        {
            GameObject source = AssetDatabase.LoadAssetAtPath<GameObject>(SourcePrefabPath);
            DismantleModularCharacter(source);
        }

        public static void DismantleModularCharacter(GameObject sourcePrefabAsset)
        {
            if (!TryResolveSourcePrefab(sourcePrefabAsset, out GameObject source, out string assetPath))
            {
                return;
            }

            string sourceName = Path.GetFileNameWithoutExtension(assetPath);
            if (!EditorUtility.DisplayDialog(
                    "预制体处理工具",
                    $"将拆解：{sourceName}\n\n" +
                    $"• 部件 → {RolePartRoot}/{{槽位名}}/{{部件名}}.prefab\n" +
                    $"• {RoleFolder}/Role_*_F / Role_*_M：在 Body/Head 等同级新增 Root，写入原骨架空壳\n" +
                    "  （_F 隐藏 Male_Parts，_M 隐藏 Female_Parts；不改动 Body/Head 挂点）\n\n" +
                    "会覆盖同名资源，是否继续？",
                    "拆解",
                    "取消"))
            {
                return;
            }

            EnsureFolder("Assets/Bundles/Unit", "RolePart");
            EnsureFolder("Assets/Bundles/Unit", "Role");

            int partCount = 0;
            int slotCount = 0;
            int roleCount = 0;

            try
            {
                partCount = ExportAllParts(source);
                SyncDefaultPartsConfigFromSource(source, sourceName);
                slotCount = CountSlotFolders();
                roleCount = ApplySyntyRigToRolePrefabs(source);
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }

            if (File.Exists(LegacySkeletonPath))
            {
                AssetDatabase.DeleteAsset(LegacySkeletonPath);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog(
                "预制体处理工具",
                $"拆解完成。\n槽位文件夹：{slotCount}\n部件 Prefab：{partCount}\n已处理 Role 预制体：{roleCount}",
                "确定");
        }

        static void SyncDefaultPartsConfigFromSource(GameObject source, string sourcePrefabName)
        {
            if (source == null || string.IsNullOrEmpty(sourcePrefabName))
            {
                return;
            }

            if (!SyntyModularRolePartUtil.TryParseRolePrefabGender(sourcePrefabName, out _, out _))
            {
                return;
            }

            string[] parts = RoleDefaultVisiblePartsConfigBuilder.CollectVisiblePartNamesFromSourcePrefab(
                source,
                sourcePrefabName);
            if (parts == null || parts.Length == 0)
            {
                Debug.LogWarning(
                    $"[Synty拆解] 未从源模型「{sourcePrefabName}」读到 activeSelf 部件，Role 仍使用配置表中的列表。");
                return;
            }

            if (!RoleDefaultVisiblePartsConfigImporter.TryImportEntryForDismantle(sourcePrefabName, parts))
            {
                Debug.LogWarning($"[Synty拆解] 未能将源模型部件写入配置：{sourcePrefabName}");
            }
        }

        public static bool TryResolveSourcePrefab(GameObject sourcePrefabAsset, out GameObject source, out string assetPath)
        {
            source = null;
            assetPath = null;
            if (sourcePrefabAsset == null)
            {
                EditorUtility.DisplayDialog("预制体处理工具", "请先指定要拆解的模型预制体。", "确定");
                return false;
            }

            source = sourcePrefabAsset;
            assetPath = AssetDatabase.GetAssetPath(source);
            if (string.IsNullOrEmpty(assetPath))
            {
                source = PrefabUtility.GetCorrespondingObjectFromOriginalSource(sourcePrefabAsset);
                assetPath = source != null ? AssetDatabase.GetAssetPath(source) : null;
            }

            if (string.IsNullOrEmpty(assetPath) || !assetPath.EndsWith(".prefab", System.StringComparison.OrdinalIgnoreCase))
            {
                EditorUtility.DisplayDialog("预制体处理工具", "请拖入 Project 中的角色预制体（.prefab），不支持场景实例。", "确定");
                return false;
            }

            return true;
        }

        static int ApplySyntyRigToRolePrefabs(GameObject sourcePrefab)
        {
            if (!TryBuildSkeletonTemplate(sourcePrefab, out GameObject skeletonTemplate))
            {
                return 0;
            }

            Avatar modularAvatar = LoadModularCharactersAvatar();
            if (modularAvatar == null)
            {
                UnityEngine.Object.DestroyImmediate(skeletonTemplate);
                Debug.LogError($"[Synty拆解] 未找到 Avatar：{ModularCharactersAvatarName}（{ModularCharactersFbxPath}）");
                return 0;
            }

            string defaultPartsConfigPath = RoleDefaultVisiblePartsConfig.DefaultAssetPath;
            AssetDatabase.ImportAsset(defaultPartsConfigPath, ImportAssetOptions.ForceUpdate);
            RoleDefaultVisiblePartsConfig defaultPartsConfig =
                AssetDatabase.LoadAssetAtPath<RoleDefaultVisiblePartsConfig>(defaultPartsConfigPath);
            if (defaultPartsConfig == null)
            {
                Debug.LogWarning(
                    $"[Synty拆解] 未找到 {defaultPartsConfigPath}，Role 将不挂载默认部件。");
            }

            Dictionary<string, string> rolePartPrefabPaths = BuildRolePartPrefabPathIndex();
            int processed = 0;
            string[] guids = AssetDatabase.FindAssets("t:Prefab", new[] { RoleFolder });

            try
            {
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

                    EditorUtility.DisplayProgressBar("写入 Role 骨架", fileName, processed / (float)Mathf.Max(1, guids.Length));

                    if (ApplyRigToSingleRolePrefab(
                            path,
                            fileName,
                            isFemale,
                            skeletonTemplate,
                            modularAvatar,
                            defaultPartsConfig,
                            rolePartPrefabPaths))
                    {
                        processed++;
                    }
                }
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(skeletonTemplate);
            }

            return processed;
        }

        /// <summary>
        /// 等价于原 ModularCharacter_Skeleton：去 SMR 的 ModularCharacter_01（子级含 Modular_Characters + 骨骼 Root）。
        /// </summary>
        static bool TryBuildSkeletonTemplate(GameObject sourcePrefab, out GameObject skeletonTemplate)
        {
            skeletonTemplate = null;

            skeletonTemplate = PrefabUtility.InstantiatePrefab(sourcePrefab) as GameObject;
            skeletonTemplate.hideFlags = HideFlags.HideAndDontSave;

            foreach (var smr in skeletonTemplate.GetComponentsInChildren<SkinnedMeshRenderer>(true))
            {
                UnityEngine.Object.DestroyImmediate(smr.gameObject);
            }

            if (FindDirectChild(skeletonTemplate.transform, "Root") == null ||
                FindDirectChild(skeletonTemplate.transform, "Modular_Characters") == null)
            {
                UnityEngine.Object.DestroyImmediate(skeletonTemplate);
                skeletonTemplate = null;
                Debug.LogError("[Synty拆解] 源预制体缺少骨骼 Root 或 Modular_Characters");
                return false;
            }

            foreach (var anim in skeletonTemplate.GetComponentsInChildren<Animator>(true))
            {
                UnityEngine.Object.DestroyImmediate(anim);
            }

            SetActiveIfExists(skeletonTemplate.transform, "Modular_Characters/Female_Parts");
            SetActiveIfExists(skeletonTemplate.transform, "Modular_Characters/Male_Parts");
            SetActiveIfExists(skeletonTemplate.transform, "Modular_Characters/All_Gender_Parts");
            return true;
        }

        static bool ApplyRigToSingleRolePrefab(
            string prefabPath,
            string prefabName,
            bool isFemale,
            GameObject skeletonTemplate,
            Avatar modularAvatar,
            RoleDefaultVisiblePartsConfig defaultPartsConfig,
            Dictionary<string, string> rolePartPrefabPaths)
        {
            GameObject root = PrefabUtility.LoadPrefabContents(prefabPath);
            try
            {
                RemoveDirectChildByName(root.transform, "Root");
                RemoveDirectChildByName(root.transform, "Modular_Characters");

                GameObject rootAnchor = new GameObject("Root");
                rootAnchor.transform.SetParent(root.transform, false);
                ResetLocalTransform(rootAnchor.transform);
                MountSkeletonUnderRoot(rootAnchor.transform, skeletonTemplate.transform);

                ApplyGenderPartsVisibility(FindDirectChild(rootAnchor.transform, "Modular_Characters"), isFemale);

                var rc = root.GetComponent<ReferenceCollector>();
                if (rc != null)
                {
                    rc.Add("Root", rootAnchor);
                }

                SetupRootAnimator(root, rootAnchor, prefabName, modularAvatar);
                MountDefaultVisibleParts(rootAnchor, prefabName, rolePartPrefabPaths);

                if (root.GetComponent<SyntyModularCharacterSkinnedBinder>() == null)
                {
                    root.AddComponent<SyntyModularCharacterSkinnedBinder>();
                }

                Transform skeletonBoneRoot = SyntySkinnedBoneBinder.ResolveSkeletonBoneRoot(root.transform);
                SyntySkinnedBoneBinder.RebindAllSkinnedParts(root.transform, skeletonBoneRoot);

                PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
                return true;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[Synty拆解] 处理 {prefabName} 失败: {e.Message}");
                return false;
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        static void RemoveDirectChildByName(Transform roleRoot, string childName)
        {
            for (int i = roleRoot.childCount - 1; i >= 0; i--)
            {
                Transform child = roleRoot.GetChild(i);
                if (child.name == childName)
                {
                    UnityEngine.Object.DestroyImmediate(child.gameObject);
                }
            }
        }

        static Dictionary<string, string> BuildRolePartPrefabPathIndex()
        {
            var map = new Dictionary<string, string>();
            string[] guids = AssetDatabase.FindAssets("t:Prefab", new[] { RolePartRoot });
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                string fileName = Path.GetFileNameWithoutExtension(path);
                if (string.IsNullOrEmpty(fileName) || map.ContainsKey(fileName))
                {
                    continue;
                }

                map.Add(fileName, path);
            }

            return map;
        }

        static void MountDefaultVisibleParts(
            GameObject rootAnchor,
            string prefabName,
            Dictionary<string, string> rolePartPrefabPaths)
        {
            if (rootAnchor == null)
            {
                return;
            }

            bool roleIsFemale = prefabName.EndsWith("_F", System.StringComparison.Ordinal);
            bool roleIsMale = prefabName.EndsWith("_M", System.StringComparison.Ordinal);
            string[] partNames = RoleDefaultVisiblePartsConfigImporter.TryGetPartNames(prefabName);
            if (partNames == null || partNames.Length == 0)
            {
                Debug.LogWarning($"[Synty拆解] {prefabName} 在配置中无默认部件，跳过挂载。");
                return;
            }

            if (roleIsFemale || roleIsMale)
            {
                partNames = SyntyModularRolePartUtil.FilterPartsForRoleGender(partNames, roleIsFemale);
            }

            Transform modularRoot = FindDirectChild(rootAnchor.transform, "Modular_Characters");
            Transform roleRoot = rootAnchor.transform.parent != null ? rootAnchor.transform.parent : rootAnchor.transform;
            Transform skeletonRoot = SyntySkinnedBoneBinder.ResolveSkeletonBoneRoot(roleRoot);
            if (skeletonRoot == null)
            {
                skeletonRoot = FindDirectChild(rootAnchor.transform, "Root") ?? rootAnchor.transform;
            }

            int mounted = 0;
            foreach (string partName in partNames)
            {
                if (string.IsNullOrEmpty(partName))
                {
                    continue;
                }

                string prefabPath = SyntyModularRolePartUtil.ResolveRolePartPrefabPath(
                    partName,
                    roleIsFemale,
                    rolePartPrefabPaths,
                    RolePartRoot);
                if (string.IsNullOrEmpty(prefabPath))
                {
                    Debug.LogWarning(
                        $"[Synty拆解] {prefabName} 默认部件未找到与性别匹配的 RolePart：{partName}（Female_* / Male_*）");
                    continue;
                }

                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
                if (prefab == null)
                {
                    continue;
                }

                GameObject instance = PrefabUtility.InstantiatePrefab(prefab, rootAnchor.transform) as GameObject;
                if (instance == null)
                {
                    continue;
                }

                if (PrefabUtility.IsPartOfPrefabInstance(instance))
                {
                    PrefabUtility.UnpackPrefabInstance(
                        instance,
                        PrefabUnpackMode.Completely,
                        InteractionMode.AutomatedAction);
                }

                instance.name = partName;
                SyntySkinnedPartMetadata metadata = instance.GetComponent<SyntySkinnedPartMetadata>();
                Transform slotParent = ResolveSlotParent(rootAnchor.transform, modularRoot, metadata?.slotPath);
                if (slotParent == null)
                {
                    Debug.LogWarning($"[Synty拆解] {prefabName} 槽位未找到：{metadata?.slotPath ?? partName}");
                    slotParent = rootAnchor.transform;
                }

                instance.transform.SetParent(slotParent, false);
                ResetLocalTransform(instance.transform);
                instance.SetActive(true);
                EnsurePartSlotActive(slotParent, modularRoot);

                SkinnedMeshRenderer smr = instance.GetComponent<SkinnedMeshRenderer>();
                if (smr != null && metadata != null)
                {
                    smr.enabled = true;
                    SyntySkinnedBoneBinder.Bind(smr, skeletonRoot, metadata);
                }

                mounted++;
            }

            Debug.Log($"[Synty拆解] {prefabName} 已挂载默认部件 {mounted}/{partNames.Length}。");
        }

        static Transform ResolveSlotParent(Transform rootAnchor, Transform modularRoot, string slotPath)
        {
            if (string.IsNullOrEmpty(slotPath))
            {
                return null;
            }

            Transform slot = FindByPath(rootAnchor, slotPath);
            if (slot != null)
            {
                return slot;
            }

            const string prefix = "Modular_Characters/";
            if (modularRoot != null && slotPath.StartsWith(prefix, System.StringComparison.Ordinal))
            {
                slot = FindByPath(modularRoot, slotPath.Substring(prefix.Length));
            }

            return slot;
        }

        static void EnsurePartSlotActive(Transform slotParent, Transform modularRoot)
        {
            if (slotParent == null || modularRoot == null)
            {
                return;
            }

            Transform female = FindDirectChild(modularRoot, "Female_Parts");
            Transform male = FindDirectChild(modularRoot, "Male_Parts");
            Transform allGender = FindDirectChild(modularRoot, "All_Gender_Parts");

            for (Transform walk = slotParent; walk != null && walk != modularRoot.parent; walk = walk.parent)
            {
                walk.gameObject.SetActive(true);
            }

            if (female != null && slotParent.IsChildOf(female))
            {
                female.gameObject.SetActive(true);
            }

            if (male != null && slotParent.IsChildOf(male))
            {
                male.gameObject.SetActive(true);
            }

            if (allGender != null && slotParent.IsChildOf(allGender))
            {
                allGender.gameObject.SetActive(true);
            }
        }

        static void MountSkeletonUnderRoot(Transform rootAnchor, Transform skeletonRoot)
        {
            for (int i = 0; i < skeletonRoot.childCount; i++)
            {
                Transform child = skeletonRoot.GetChild(i);
                GameObject copy = UnityEngine.Object.Instantiate(child.gameObject, rootAnchor);
                copy.name = child.name;
                ResetLocalTransform(copy.transform);
            }
        }

        static void ApplyGenderPartsVisibility(Transform modularRoot, bool isFemale)
        {
            if (modularRoot == null)
            {
                return;
            }

            Transform female = FindDirectChild(modularRoot, "Female_Parts");
            Transform male = FindDirectChild(modularRoot, "Male_Parts");
            Transform allGender = FindDirectChild(modularRoot, "All_Gender_Parts");

            if (female != null)
            {
                female.gameObject.SetActive(isFemale);
            }

            if (male != null)
            {
                male.gameObject.SetActive(!isFemale);
            }

            if (allGender != null)
            {
                allGender.gameObject.SetActive(true);
            }
        }

        static Avatar LoadModularCharactersAvatar()
        {
            UnityEngine.Object[] assets = AssetDatabase.LoadAllAssetsAtPath(ModularCharactersFbxPath);
            if (assets == null || assets.Length == 0)
            {
                return null;
            }

            Avatar fallback = null;
            foreach (UnityEngine.Object asset in assets)
            {
                if (asset is not Avatar avatar)
                {
                    continue;
                }

                if (avatar.name == ModularCharactersAvatarName)
                {
                    return avatar;
                }

                fallback ??= avatar;
            }

            return fallback;
        }

        static RuntimeAnimatorController ResolveRoleAnimatorController(string prefabName)
        {
            string[] parts = prefabName.Split('_');
            if (parts.Length < 4 || parts[0] != "Role")
            {
                return null;
            }

            string className = parts[1];
            string gender = parts[parts.Length - 1];
            if (gender != "F" && gender != "M")
            {
                return null;
            }

            string assetPath = $"{RoleAnimatorFolder}/Role{className}{gender}Ctrl.overrideController";
            return AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(assetPath);
        }

        static void SetupRootAnimator(GameObject roleRoot, GameObject rootAnchor, string prefabName, Avatar modularAvatar)
        {
            if (modularAvatar == null || rootAnchor == null)
            {
                return;
            }

            var legacyRoleAnimator = roleRoot.GetComponent<Animator>();
            if (legacyRoleAnimator != null)
            {
                UnityEngine.Object.DestroyImmediate(legacyRoleAnimator);
            }

            foreach (var anim in rootAnchor.GetComponentsInChildren<Animator>(true))
            {
                if (anim.gameObject != rootAnchor)
                {
                    UnityEngine.Object.DestroyImmediate(anim);
                }
            }

            var animator = rootAnchor.GetComponent<Animator>();
            if (animator == null)
            {
                animator = rootAnchor.AddComponent<Animator>();
            }

            animator.avatar = modularAvatar;
            animator.runtimeAnimatorController = ResolveRoleAnimatorController(prefabName);
            animator.applyRootMotion = false;

            if (animator.runtimeAnimatorController == null)
            {
                Debug.LogWarning($"[Synty拆解] 未找到 AnimatorController：{prefabName}");
            }
        }

        static void ResetLocalTransform(Transform t)
        {
            t.localPosition = Vector3.zero;
            t.localRotation = Quaternion.identity;
            t.localScale = Vector3.one;
        }

        static int ExportAllParts(GameObject sourcePrefab)
        {
            int total = 0;
            GameObject tempRoot = PrefabUtility.InstantiatePrefab(sourcePrefab) as GameObject;
            tempRoot.hideFlags = HideFlags.HideAndDontSave;

            try
            {
                ExportPartsUnder(
                    tempRoot.transform,
                    "Modular_Characters/Female_Parts",
                    ref total);
                ExportPartsUnder(
                    tempRoot.transform,
                    "Modular_Characters/Male_Parts",
                    ref total);
                ExportAllGenderParts(tempRoot.transform, ref total);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(tempRoot);
            }

            return total;
        }

        static void ExportAllGenderParts(Transform characterRoot, ref int total)
        {
            Transform allGender = FindByPath(characterRoot, "Modular_Characters/All_Gender_Parts");
            if (allGender == null)
            {
                return;
            }

            for (int i = 0; i < allGender.childCount; i++)
            {
                Transform child = allGender.GetChild(i);
                if (child.name == "All_00_HeadCoverings")
                {
                    for (int j = 0; j < child.childCount; j++)
                    {
                        Transform sub = child.GetChild(j);
                        ExportSlotRecursive(
                            sub,
                            $"Modular_Characters/All_Gender_Parts/All_00_HeadCoverings/{sub.name}",
                            ref total);
                    }
                }
                else
                {
                    ExportSlotRecursive(
                        child,
                        $"Modular_Characters/All_Gender_Parts/{child.name}",
                        ref total);
                }
            }
        }

        static void ExportPartsUnder(Transform characterRoot, string partsRootPath, ref int total)
        {
            Transform partsRoot = FindByPath(characterRoot, partsRootPath);
            if (partsRoot == null)
            {
                Debug.LogWarning("[Synty拆解] 未找到: " + partsRootPath);
                return;
            }

            for (int i = 0; i < partsRoot.childCount; i++)
            {
                Transform slotNode = partsRoot.GetChild(i);
                ExportSlotRecursive(slotNode, $"{partsRootPath}/{slotNode.name}", ref total);
            }
        }

        static void ExportSlotRecursive(Transform slotFolder, string slotPath, ref int total)
        {
            if (slotFolder == null)
            {
                return;
            }

            if (!HasDirectSkinnedChild(slotFolder))
            {
                for (int i = 0; i < slotFolder.childCount; i++)
                {
                    Transform child = slotFolder.GetChild(i);
                    ExportSlotRecursive(child, $"{slotPath}/{child.name}", ref total);
                }

                return;
            }

            string slotFolderName = SanitizeFolderName(slotFolder.name);
            string exportDir = $"{RolePartRoot}/{slotFolderName}";
            EnsureAssetFolder(exportDir);

            for (int i = 0; i < slotFolder.childCount; i++)
            {
                Transform child = slotFolder.GetChild(i);
                var smr = child.GetComponent<SkinnedMeshRenderer>();
                if (smr == null)
                {
                    continue;
                }

                EditorUtility.DisplayProgressBar("拆解 ModularCharacter_01", $"{slotFolderName}/{child.name}", total % 1000 / 1000f);

                GameObject clone = UnityEngine.Object.Instantiate(child.gameObject);
                clone.name = child.name;
                clone.SetActive(true);

                var metadata = clone.GetComponent<SyntySkinnedPartMetadata>();
                if (metadata == null)
                {
                    metadata = clone.AddComponent<SyntySkinnedPartMetadata>();
                }

                FillMetadata(smr, metadata, slotPath);

                string savePath = $"{exportDir}/{child.name}.prefab";
                if (File.Exists(savePath))
                {
                    AssetDatabase.DeleteAsset(savePath);
                }

                PrefabUtility.SaveAsPrefabAsset(clone, savePath);
                UnityEngine.Object.DestroyImmediate(clone);
                total++;
            }
        }

        static void SetActiveIfExists(Transform root, string path)
        {
            Transform t = FindByPath(root, path);
            if (t != null)
            {
                t.gameObject.SetActive(true);
            }
        }

        static void FillMetadata(SkinnedMeshRenderer smr, SyntySkinnedPartMetadata metadata, string slotPath)
        {
            metadata.slotPath = slotPath;
            if (smr == null || smr.bones == null || smr.bones.Length == 0)
            {
                return;
            }

            metadata.boneNames = new string[smr.bones.Length];
            for (int i = 0; i < smr.bones.Length; i++)
            {
                metadata.boneNames[i] = smr.bones[i] != null ? smr.bones[i].name : string.Empty;
            }

            metadata.rootBoneName = smr.rootBone != null ? smr.rootBone.name : "Hips";
        }

        static int CountSlotFolders()
        {
            if (!AssetDatabase.IsValidFolder(RolePartRoot))
            {
                return 0;
            }

            string[] sub = AssetDatabase.GetSubFolders(RolePartRoot);
            return sub?.Length ?? 0;
        }

        static bool HasDirectSkinnedChild(Transform parent)
        {
            for (int i = 0; i < parent.childCount; i++)
            {
                if (parent.GetChild(i).GetComponent<SkinnedMeshRenderer>() != null)
                {
                    return true;
                }
            }

            return false;
        }

        static Transform FindByPath(Transform root, string path)
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

        static Transform FindDirectChild(Transform parent, string childName)
        {
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

        static string SanitizeFolderName(string name)
        {
            foreach (char c in Path.GetInvalidFileNameChars())
            {
                name = name.Replace(c, '_');
            }

            return name;
        }

        static void EnsureFolder(string parent, string name)
        {
            string path = $"{parent}/{name}";
            if (!AssetDatabase.IsValidFolder(path))
            {
                AssetDatabase.CreateFolder(parent, name);
            }
        }

        static void EnsureAssetFolder(string assetPath)
        {
            if (AssetDatabase.IsValidFolder(assetPath))
            {
                return;
            }

            string parent = Path.GetDirectoryName(assetPath)?.Replace('\\', '/');
            string folderName = Path.GetFileName(assetPath);
            if (!string.IsNullOrEmpty(parent) && !AssetDatabase.IsValidFolder(parent))
            {
                string[] segments = assetPath.Replace('\\', '/').Split('/');
                string current = segments[0];
                for (int i = 1; i < segments.Length; i++)
                {
                    string next = current + "/" + segments[i];
                    if (!AssetDatabase.IsValidFolder(next))
                    {
                        AssetDatabase.CreateFolder(current, segments[i]);
                    }

                    current = next;
                }
            }
            else if (!string.IsNullOrEmpty(parent))
            {
                AssetDatabase.CreateFolder(parent, folderName);
            }
        }
    }
}
