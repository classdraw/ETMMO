using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ET
{
    /// <summary>
    /// 预制体批量处理工具入口。
    /// </summary>
    public class PrefabToolsWindow : EditorWindow
    {
        private const string AvatarPrefabOutputRoot = "Assets/Bundles/Avatar";

        [MenuItem("Tools/预制体处理工具合集", false, 50)]
        public static void Open()
        {
            var w = GetWindow<PrefabToolsWindow>(true, "预制体处理工具", true);
            w.minSize = new Vector2(440, 480);
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

                if (GUILayout.Button("清空 SpriteRenderer 引用图（含子物体，sprite 非空则置 null）", GUILayout.Height(34)))
                {
                    ClearSpriteRendererSpritesFromSelectedPrefabs();
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

            EditorGUILayout.Space(12);
            EditorGUILayout.LabelField("场景", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "仅编辑态：Hierarchy 中当前「活动」场景（EditorSceneManager.GetActiveScene），不处理其它已加载场景；退出 Play 后使用。含未激活物体。Tag 设为「NavMesh」需在 TagManager 中存在。",
                MessageType.Info);
            if (GUILayout.Button("当前活动场景：Collider 物体 Tag → NavMesh", GUILayout.Height(34)))
            {
                SetNavMeshTagOnEditorActiveSceneColliderObjects();
            }

            EditorGUILayout.Space(12);
            EditorGUILayout.LabelField("Avatar 精灵预制体", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "在 Project 中选中文件夹（可含子目录），执行前会先删除整个 " + AvatarPrefabOutputRoot + " 再重新生成。\n" +
                "非武器：路径叶子决定目录名（如 …/5_Armor → Armor）。「Armor 1」「Armor_1」会规范成 Armor 并与连续重复段合并；纯数字文件夹段忽略。武器：Weapons/Sword。\n" +
                "预制体命名：单子 Sprite → 输出目录最后一级名_Sprite名；多子 Sprite → 最后一级名_纹理文件名_子Sprite名（如 Sword_New_Weapon_01、多切片时 Sword_New_Weapon_01_Front）。",
                MessageType.Info);

            int folderCount = CountSelectedAssetFolders();
            EditorGUILayout.LabelField("当前选中的文件夹数量", folderCount.ToString());

            using (new EditorGUI.DisabledScope(folderCount == 0))
            {
                if (GUILayout.Button("从选中文件夹生成 Avatar 精灵预制体", GUILayout.Height(34)))
                {
                    GenerateAvatarSpritePrefabsFromSelectedFolders();
                }
            }
        }

        private static int CountSelectedPrefabPaths()
        {
            return CollectSelectedPrefabPaths().Count;
        }

        private static int CountSelectedAssetFolders()
        {
            return CollectSelectedAssetFolderPaths().Count;
        }

        private static List<string> CollectSelectedAssetFolderPaths()
        {
            var set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (UnityEngine.Object o in Selection.objects)
            {
                if (o == null)
                {
                    continue;
                }

                string path = AssetDatabase.GetAssetPath(o);
                if (string.IsNullOrEmpty(path) || !AssetDatabase.IsValidFolder(path))
                {
                    continue;
                }

                set.Add(path);
            }

            return new List<string>(set);
        }

        private const string NavMeshColliderTag = "NavMesh";

        /// <summary>
        /// 仅处理编辑器当前活动场景（与 Hierarchy 加粗场景一致），不用 SceneManager，避免与 ET.Scene 类型名冲突须写全名。
        /// </summary>
        private static void SetNavMeshTagOnEditorActiveSceneColliderObjects()
        {
            if (EditorApplication.isPlaying)
            {
                EditorUtility.DisplayDialog("预制体处理工具",
                    "请在退出 Play 模式后使用。\n本功能只处理编辑态下当前活动场景，不会在运行时改场景。",
                    "确定");
                return;
            }

            UnityEngine.SceneManagement.Scene unityScene = EditorSceneManager.GetActiveScene();
            if (!unityScene.IsValid() || !unityScene.isLoaded)
            {
                EditorUtility.DisplayDialog("预制体处理工具", "当前没有已加载的有效活动场景。", "确定");
                return;
            }

            GameObject[] roots = unityScene.GetRootGameObjects();
            var visited = new HashSet<int>();
            int colliderObjectCount = 0;
            int tagChanged = 0;

            Undo.IncrementCurrentGroup();
            int undoGroup = Undo.GetCurrentGroup();
            try
            {
                foreach (GameObject root in roots)
                {
                    if (root == null)
                    {
                        continue;
                    }

                    Collider[] colliders = root.GetComponentsInChildren<Collider>(true);
                    foreach (Collider col in colliders)
                    {
                        if (col == null)
                        {
                            continue;
                        }

                        GameObject go = col.gameObject;
                        if (!visited.Add(go.GetInstanceID()))
                        {
                            continue;
                        }

                        colliderObjectCount++;
                        if (go.CompareTag(NavMeshColliderTag))
                        {
                            continue;
                        }

                        Undo.RecordObject(go, "Tag → NavMesh (Collider)");
                        go.tag = NavMeshColliderTag;
                        tagChanged++;
                    }
                }
            }
            catch (UnityException e)
            {
                Undo.RevertAllDownToGroup(undoGroup);
                EditorUtility.DisplayDialog("预制体处理工具",
                    $"设置 Tag 失败。请确认 Project Settings → Tags and Layers 中存在「{NavMeshColliderTag}」标签。\n{e.Message}",
                    "确定");
                return;
            }

            Undo.SetCurrentGroupName("Collider 物体 Tag → NavMesh");
            EditorUtility.DisplayDialog("预制体处理工具",
                $"活动场景：{unityScene.name}\n带 Collider 的物体（去重）：{colliderObjectCount} 个\nTag 已改为「{NavMeshColliderTag}」：{tagChanged} 个",
                "确定");
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

        private static void ClearSpriteRendererSpritesFromSelectedPrefabs()
        {
            List<string> paths = CollectSelectedPrefabPaths();
            if (paths.Count == 0)
            {
                EditorUtility.DisplayDialog("预制体处理工具", "请先在 Project 中选中至少一个 .prefab 资源。", "确定");
                return;
            }

            if (!EditorUtility.DisplayDialog("预制体处理工具",
                    $"将对 {paths.Count} 个预制体递归处理：所有 SpriteRenderer 上 sprite 不为空的统一设为 null。\n是否继续？",
                    "确定", "取消"))
            {
                return;
            }

            int totalCleared = 0;
            int modifiedPrefabs = 0;

            AssetDatabase.StartAssetEditing();
            try
            {
                foreach (string path in paths)
                {
                    GameObject root = PrefabUtility.LoadPrefabContents(path);
                    try
                    {
                        int n = ClearSpriteRendererSpritesRecursive(root);
                        if (n > 0)
                        {
                            PrefabUtility.SaveAsPrefabAsset(root, path);
                            totalCleared += n;
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
                $"已扫描 {paths.Count} 个预制体，其中 {modifiedPrefabs} 个有修改；共清空 {totalCleared} 处 SpriteRenderer.sprite。",
                "确定");
        }

        /// <summary>递归自身及子物体：将 sprite 已赋值的 <see cref="SpriteRenderer"/> 的 sprite 设为 null。</summary>
        private static int ClearSpriteRendererSpritesRecursive(GameObject go)
        {
            int cleared = 0;
            SpriteRenderer[] renderers = go.GetComponents<SpriteRenderer>();
            for (int i = 0; i < renderers.Length; i++)
            {
                SpriteRenderer sr = renderers[i];
                if (sr != null && sr.sprite != null)
                {
                    sr.sprite = null;
                    EditorUtility.SetDirty(sr);
                    cleared++;
                }
            }

            Transform t = go.transform;
            for (int c = 0; c < t.childCount; c++)
            {
                cleared += ClearSpriteRendererSpritesRecursive(t.GetChild(c).gameObject);
            }

            return cleared;
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

        private static void GenerateAvatarSpritePrefabsFromSelectedFolders()
        {
            List<string> roots = CollectSelectedAssetFolderPaths();
            if (roots.Count == 0)
            {
                EditorUtility.DisplayDialog("预制体处理工具", "请先在 Project 中选中至少一个文件夹。", "确定");
                return;
            }

            if (!EditorUtility.DisplayDialog("预制体处理工具",
                    $"将先清空 {AvatarPrefabOutputRoot}，再从 {roots.Count} 个文件夹（含子目录）收集 Sprite 并重新生成预制体。\n是否继续？",
                    "确定", "取消"))
            {
                return;
            }

            WipeAndRecreateAvatarRoot();

            int created = 0;
            int skipped = 0;
            int failed = 0;

            AssetDatabase.StartAssetEditing();
            try
            {
                int rootIndex = 0;
                foreach (string root in roots)
                {
                    rootIndex++;
                    string[] guids = AssetDatabase.FindAssets("t:Sprite", new[] { root });
                    for (int i = 0; i < guids.Length; i++)
                    {
                        string assetPath = AssetDatabase.GUIDToAssetPath(guids[i]);
                        EditorUtility.DisplayProgressBar("生成 Avatar 预制体",
                            assetPath,
                            (rootIndex - 1 + (float)i / Mathf.Max(1, guids.Length)) / roots.Count);

                        List<Sprite> spritesAtPath = LoadSpritesAtAssetPath(assetPath);
                        if (spritesAtPath.Count == 0)
                        {
                            skipped++;
                            continue;
                        }

                        string spriteDir = Path.GetDirectoryName(assetPath);
                        if (string.IsNullOrEmpty(spriteDir))
                        {
                            skipped++;
                            continue;
                        }

                        spriteDir = spriteDir.Replace('\\', '/');
                        string rootNorm = root.Replace('\\', '/').TrimEnd('/');
                        if (!spriteDir.StartsWith(rootNorm, StringComparison.OrdinalIgnoreCase))
                        {
                            skipped++;
                            continue;
                        }

                        string outputSubPath = BuildAvatarOutputSubPath(rootNorm, spriteDir);
                        string outDir = $"{AvatarPrefabOutputRoot}/{outputSubPath}";

                        string textureBaseName = Path.GetFileNameWithoutExtension(assetPath);
                        bool multiSliceInTexture = spritesAtPath.Count > 1;
                        string folderNameToken = GetLastPathSegment(outputSubPath);

                        try
                        {
                            EnsureAssetFolderExists(outDir);

                            if (multiSliceInTexture)
                            {
                                foreach (Sprite sprite in spritesAtPath)
                                {
                                    try
                                    {
                                        string prefabName = $"{folderNameToken}_{textureBaseName}_{sprite.name}";
                                        SaveAvatarSpritePrefab(outDir, prefabName, sprite);
                                        created++;
                                    }
                                    catch (Exception e)
                                    {
                                        failed++;
                                        Debug.LogError($"[预制体工具] Avatar 生成失败: {assetPath} ({sprite.name})\n{e}");
                                    }
                                }
                            }
                            else
                            {
                                foreach (Sprite sprite in spritesAtPath)
                                {
                                    try
                                    {
                                        string prefabName = $"{folderNameToken}_{sprite.name}";
                                        SaveAvatarSpritePrefab(outDir, prefabName, sprite);
                                        created++;
                                    }
                                    catch (Exception e)
                                    {
                                        failed++;
                                        Debug.LogError($"[预制体工具] Avatar 生成失败: {assetPath} ({sprite.name})\n{e}");
                                    }
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            failed++;
                            Debug.LogError($"[预制体工具] Avatar 目录或批量生成失败: {assetPath}\n{e}");
                        }
                    }
                }
            }
            finally
            {
                AssetDatabase.StopAssetEditing();
                EditorUtility.ClearProgressBar();
            }

            AssetDatabase.Refresh();
            PruneEmptyFoldersUnderAvatarRoot();
            EditorUtility.DisplayDialog("预制体处理工具",
                $"Avatar 精灵预制体生成结束。\n写入/覆盖：{created} 个预制体\n跳过：{skipped}\n失败：{failed}",
                "确定");
        }

        private static void PruneEmptyFoldersUnderAvatarRoot()
        {
            if (!AssetDatabase.IsValidFolder(AvatarPrefabOutputRoot))
            {
                return;
            }

            string relativeFromAssets = AvatarPrefabOutputRoot.Substring("Assets/".Length);
            string rootFull = Path.Combine(Application.dataPath, relativeFromAssets);
            if (!Directory.Exists(rootFull))
            {
                return;
            }

            bool removed;
            do
            {
                removed = false;
                string[] dirs = Directory.GetDirectories(rootFull, "*", SearchOption.AllDirectories);
                var sorted = new List<string>(dirs);
                sorted.Sort((a, b) => b.Length.CompareTo(a.Length));

                foreach (string fullDir in sorted)
                {
                    bool hasSubdir = false;
                    foreach (string _ in Directory.EnumerateDirectories(fullDir))
                    {
                        hasSubdir = true;
                        break;
                    }

                    if (hasSubdir)
                    {
                        continue;
                    }

                    bool hasNonMetaFile = false;
                    foreach (string file in Directory.EnumerateFiles(fullDir))
                    {
                        if (file.EndsWith(".meta", StringComparison.OrdinalIgnoreCase))
                        {
                            continue;
                        }

                        hasNonMetaFile = true;
                        break;
                    }

                    if (hasNonMetaFile)
                    {
                        continue;
                    }

                    string tail = fullDir.Substring(Application.dataPath.Length).TrimStart(Path.DirectorySeparatorChar, '/');
                    string assetPath = Path.Combine("Assets", tail).Replace('\\', '/');
                    if (AssetDatabase.IsValidFolder(assetPath))
                    {
                        AssetDatabase.DeleteAsset(assetPath);
                        removed = true;
                    }
                }
            }
            while (removed);

            AssetDatabase.Refresh();
        }

        private static void WipeAndRecreateAvatarRoot()
        {
            if (AssetDatabase.IsValidFolder(AvatarPrefabOutputRoot))
            {
                AssetDatabase.DeleteAsset(AvatarPrefabOutputRoot);
            }

            AssetDatabase.Refresh();
            EnsureAssetFolderExists(AvatarPrefabOutputRoot);
        }

        private static string GetNonWeaponAvatarOutputFolderName(List<string> filtered)
        {
            if (filtered.Count == 0)
            {
                return "Misc";
            }

            string leaf = filtered[filtered.Count - 1];
            if (leaf.Equals("Unit", StringComparison.OrdinalIgnoreCase) ||
                leaf.Equals("Horse", StringComparison.OrdinalIgnoreCase))
            {
                return "Misc";
            }

            return leaf;
        }

        private static List<string> CollectAvatarPathSegmentsFiltered(string rootNorm, string spriteDir)
        {
            rootNorm = rootNorm.Replace('\\', '/').TrimEnd('/');
            spriteDir = spriteDir.Replace('\\', '/').TrimEnd('/');

            string relDir = string.Empty;
            if (spriteDir.Length > rootNorm.Length && spriteDir.StartsWith(rootNorm, StringComparison.OrdinalIgnoreCase))
            {
                relDir = spriteDir.Substring(rootNorm.Length).TrimStart('/');
            }

            var segments = new List<string>();
            string rootLeafName = Path.GetFileName(rootNorm);
            if (!string.IsNullOrEmpty(rootLeafName))
            {
                segments.Add(AvatarPathSegmentNaming.StripOrderedFolderPrefix(rootLeafName));
            }

            if (!string.IsNullOrEmpty(relDir))
            {
                foreach (string part in relDir.Split('/'))
                {
                    if (string.IsNullOrEmpty(part))
                    {
                        continue;
                    }

                    segments.Add(AvatarPathSegmentNaming.StripOrderedFolderPrefix(part));
                }
            }

            var filtered = new List<string>();
            foreach (string s in segments)
            {
                if (s.Equals("Sprite", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                filtered.Add(s);
            }

            return filtered;
        }

        private static List<string> CollectAvatarPathSegmentsForAvatar(string rootNorm, string spriteDir)
        {
            return AvatarPathSegmentNaming.ApplyAvatarStylePathNormalization(CollectAvatarPathSegmentsFiltered(rootNorm, spriteDir));
        }

        private static string BuildAvatarOutputSubPath(string rootNorm, string spriteDir)
        {
            List<string> filtered = CollectAvatarPathSegmentsForAvatar(rootNorm, spriteDir);

            if (TryBuildWeaponAvatarSubPath(filtered, out string weaponSubPath))
            {
                return weaponSubPath;
            }

            return GetNonWeaponAvatarOutputFolderName(filtered);
        }

        private static bool IsWeaponRootFolderToken(string token)
        {
            return token.Equals("Weapon", StringComparison.OrdinalIgnoreCase)
                   || token.Equals("Weapons", StringComparison.OrdinalIgnoreCase);
        }

        private static bool TryBuildWeaponAvatarSubPath(List<string> filtered, out string subPath)
        {
            for (int i = 0; i < filtered.Count; i++)
            {
                if (!IsWeaponRootFolderToken(filtered[i]))
                {
                    continue;
                }

                if (i + 1 >= filtered.Count)
                {
                    subPath = "Weapons/Misc";
                    return true;
                }

                var parts = new List<string> { "Weapons" };
                for (int j = i + 1; j < filtered.Count; j++)
                {
                    parts.Add(filtered[j]);
                }

                subPath = string.Join("/", parts);
                return true;
            }

            subPath = null;
            return false;
        }

        private static string GetLastPathSegment(string outputSubPath)
        {
            if (string.IsNullOrEmpty(outputSubPath))
            {
                return "Misc";
            }

            int idx = outputSubPath.LastIndexOf('/');
            return idx < 0 ? outputSubPath : outputSubPath.Substring(idx + 1);
        }

        private static List<Sprite> LoadSpritesAtAssetPath(string assetPath)
        {
            var list = new List<Sprite>();
            UnityEngine.Object[] assets = AssetDatabase.LoadAllAssetsAtPath(assetPath);
            foreach (UnityEngine.Object o in assets)
            {
                if (o is Sprite s)
                {
                    list.Add(s);
                }
            }

            return list;
        }

        private static void EnsureAssetFolderExists(string assetFolderPath)
        {
            assetFolderPath = assetFolderPath.Replace('\\', '/');
            if (AssetDatabase.IsValidFolder(assetFolderPath))
            {
                return;
            }

            int lastSlash = assetFolderPath.LastIndexOf('/');
            string parent = lastSlash > 0 ? assetFolderPath.Substring(0, lastSlash) : string.Empty;
            string name = lastSlash >= 0 ? assetFolderPath.Substring(lastSlash + 1) : assetFolderPath;

            if (!string.IsNullOrEmpty(parent) && !AssetDatabase.IsValidFolder(parent))
            {
                EnsureAssetFolderExists(parent);
            }

            if (!AssetDatabase.IsValidFolder(assetFolderPath))
            {
                AssetDatabase.CreateFolder(string.IsNullOrEmpty(parent) ? "Assets" : parent, name);
            }
        }

        private static void SaveAvatarSpritePrefab(string outDir, string prefabName, Sprite sprite)
        {
            string prefabPath = $"{outDir}/{prefabName}.prefab".Replace('\\', '/');
            var go = new GameObject(prefabName);
            try
            {
                SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = sprite;
                PrefabUtility.SaveAsPrefabAsset(go, prefabPath);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(go);
            }
        }
    }
}
