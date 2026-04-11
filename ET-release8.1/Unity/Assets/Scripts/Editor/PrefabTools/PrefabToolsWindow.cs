using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
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
        private const string AvatarPrefabOutputRoot = "Assets/Bundles/Avatar";

        private AvatarBindKeyAllowlistAsset allowlistAsset;

        private readonly List<AvatarBindKeyEntry> allowlistEditorEntries = new List<AvatarBindKeyEntry>();

        private string allowlistSearchFilter = string.Empty;

        private Vector2 allowlistScroll;

        private float allowlistScrollMaxHeight = 180f;

        [MenuItem("Tools/预制体处理工具合集", false, 50)]
        public static void Open()
        {
            var w = GetWindow<PrefabToolsWindow>(true, "预制体处理工具", true);
            w.minSize = new Vector2(440, 480);
        }

        private void OnEnable()
        {
            SyncAllowlistFromAsset();
        }

        private void SyncAllowlistFromAsset()
        {
            allowlistAsset = AvatarBindKeyAllowlistUtility.Load();
            allowlistEditorEntries.Clear();
            if (allowlistAsset != null)
            {
                allowlistAsset.MigrateLegacyKeysIfNeeded();
                if (allowlistAsset.entries != null)
                {
                    foreach (AvatarBindKeyEntry e in allowlistAsset.entries)
                    {
                        allowlistEditorEntries.Add(new AvatarBindKeyEntry
                        {
                            key = e.key ?? string.Empty,
                            value = e.value
                        });
                    }
                }

                allowlistScrollMaxHeight = allowlistAsset.editorListScrollHeight;
            }
            else
            {
                allowlistScrollMaxHeight = 180f;
            }
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

            EditorGUILayout.Space(12);
            DrawAvatarBindKeyAllowlistPanel();
        }

        private void DrawAvatarBindKeyAllowlistPanel()
        {
            EditorGUILayout.LabelField("角色绑点 Key / Value 白名单", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                $"资源路径：{AvatarBindKeyAllowlistUtility.DefaultAssetPath}\n" +
                "Key：与挂 SpriteRenderer 的节点名完全一致（Ordinal）。Value：整型，与 ET.Client.AvatarPartType 取值对应（通常从 0 递增）。\n" +
                "「导出 AvatarPartType」会生成可粘贴到枚举体内的成员行。保存后写入资源。",
                MessageType.Info);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("当前资源", GUILayout.Width(52));
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.ObjectField(allowlistAsset, typeof(AvatarBindKeyAllowlistAsset), false);
            EditorGUI.EndDisabledGroup();
            if (GUILayout.Button("刷新", GUILayout.Width(44)))
            {
                SyncAllowlistFromAsset();
            }

            if (GUILayout.Button("创建资源", GUILayout.Width(72)))
            {
                if (allowlistAsset == null)
                {
                    allowlistAsset = AvatarBindKeyAllowlistUtility.CreateAtDefaultPath();
                    SyncAllowlistFromAsset();
                    Selection.activeObject = allowlistAsset;
                    EditorGUIUtility.PingObject(allowlistAsset);
                }
                else
                {
                    EditorUtility.DisplayDialog("预制体处理工具", "默认路径已存在白名单资源。", "确定");
                }
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            allowlistSearchFilter = EditorGUILayout.TextField("查找", allowlistSearchFilter ?? string.Empty);
            if (GUILayout.Button("清空查找", GUILayout.Width(72)))
            {
                allowlistSearchFilter = string.Empty;
            }

            EditorGUILayout.EndHorizontal();

            float sliderH = EditorGUILayout.Slider("列表区域高度", allowlistScrollMaxHeight, 100f, 520f);
            if (Mathf.Abs(sliderH - allowlistScrollMaxHeight) > 0.01f)
            {
                allowlistScrollMaxHeight = sliderH;
            }

            allowlistScroll = EditorGUILayout.BeginScrollView(allowlistScroll, GUILayout.Height(allowlistScrollMaxHeight));

            string q = allowlistSearchFilter?.Trim() ?? string.Empty;
            int removeIndex = -1;

            for (int i = 0; i < allowlistEditorEntries.Count; i++)
            {
                AvatarBindKeyEntry row = allowlistEditorEntries[i];
                string keyStr = row.key ?? string.Empty;
                string valueStr = row.value.ToString();
                if (q.Length > 0 &&
                    keyStr.IndexOf(q, StringComparison.OrdinalIgnoreCase) < 0 &&
                    valueStr.IndexOf(q, StringComparison.OrdinalIgnoreCase) < 0)
                {
                    continue;
                }

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField($"{i + 1}.", GUILayout.Width(28));
                EditorGUILayout.LabelField("Key", GUILayout.Width(28));
                row.key = EditorGUILayout.TextField(keyStr);
                EditorGUILayout.LabelField("Value", GUILayout.Width(40));
                row.value = EditorGUILayout.IntField(row.value, GUILayout.Width(56));
                allowlistEditorEntries[i] = row;
                if (GUILayout.Button("删除", GUILayout.Width(44)))
                {
                    removeIndex = i;
                }

                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndScrollView();

            if (removeIndex >= 0)
            {
                allowlistEditorEntries.RemoveAt(removeIndex);
            }

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("添加一项", GUILayout.Height(26)))
            {
                int nextVal = 0;
                foreach (AvatarBindKeyEntry e in allowlistEditorEntries)
                {
                    if (e.value >= nextVal)
                    {
                        nextVal = e.value + 1;
                    }
                }

                allowlistEditorEntries.Add(new AvatarBindKeyEntry { key = string.Empty, value = nextVal });
            }

            if (GUILayout.Button("保存到资源", GUILayout.Height(26)))
            {
                if (allowlistAsset == null)
                {
                    allowlistAsset = AvatarBindKeyAllowlistUtility.CreateAtDefaultPath();
                }

                Undo.RecordObject(allowlistAsset, "保存角色绑点 Key 白名单");
                if (allowlistAsset.entries == null)
                {
                    allowlistAsset.entries = new List<AvatarBindKeyEntry>();
                }

                allowlistAsset.entries.Clear();
                foreach (AvatarBindKeyEntry e in allowlistEditorEntries)
                {
                    string k = e.key?.Trim() ?? string.Empty;
                    if (k.Length == 0)
                    {
                        continue;
                    }

                    allowlistAsset.entries.Add(new AvatarBindKeyEntry { key = k, value = e.value });
                }

                allowlistAsset.editorListScrollHeight = allowlistScrollMaxHeight;
                EditorUtility.SetDirty(allowlistAsset);
                AssetDatabase.SaveAssets();
                EditorUtility.DisplayDialog("预制体处理工具", "已写入 ScriptableObject 资源。", "确定");
            }

            if (GUILayout.Button("导出 AvatarPartType", GUILayout.Height(26)))
            {
                ExportAvatarPartTypeEnumSnippetToClipboard();
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.LabelField($"共 {allowlistEditorEntries.Count} 项（保存时跳过 Key 为空的行）", EditorStyles.miniLabel);
        }

        private void ExportAvatarPartTypeEnumSnippetToClipboard()
        {
            var rows = new List<(string member, int val)>();
            foreach (AvatarBindKeyEntry e in allowlistEditorEntries)
            {
                string k = e.key?.Trim() ?? string.Empty;
                if (k.Length == 0)
                {
                    continue;
                }

                rows.Add((KeyStringToEnumMemberName(k), e.value));
            }

            if (rows.Count == 0)
            {
                EditorUtility.DisplayDialog("预制体处理工具", "没有可导出的项（Key 均为空）。", "确定");
                return;
            }

            rows.Sort((a, b) => a.val.CompareTo(b.val));
            var usedNames = new HashSet<string>(StringComparer.Ordinal);
            var sb = new StringBuilder();
            sb.AppendLine("// 粘贴到 ET.Client.AvatarPartType 枚举体内，与现有成员合并；末尾请保留 Count。");
            foreach ((string member, int val) in rows)
            {
                string m = member;
                string baseName = m;
                int suf = 2;
                while (usedNames.Contains(m))
                {
                    m = baseName + "_" + suf++;
                }

                usedNames.Add(m);
                sb.AppendLine($"        {m} = {val},");
            }

            EditorGUIUtility.systemCopyBuffer = sb.ToString().TrimEnd();
            EditorUtility.DisplayDialog("预制体处理工具", $"已复制 {rows.Count} 行枚举成员到剪贴板。", "确定");
        }

        private static string KeyStringToEnumMemberName(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                return "Invalid";
            }

            var sb = new StringBuilder();
            for (int i = 0; i < key.Length; i++)
            {
                char c = key[i];
                if (char.IsLetterOrDigit(c) || c == '_')
                {
                    sb.Append(c);
                }
                else
                {
                    sb.Append('_');
                }
            }

            string s = sb.ToString();
            if (s.Length == 0)
            {
                return "Invalid";
            }

            if (char.IsDigit(s[0]))
            {
                return "K_" + s;
            }

            return s;
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
