using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace ET
{
    public class ResourceImportConfigWindow : EditorWindow
    {
        private static readonly int[] MaxSizeOptions = { 32, 64, 128, 256, 512, 1024, 2048, 4096, 8192 };

        private int selectedTemplateIndex;
        private int selectedRuleIndex = -1;
        private Vector2 templateListScroll;
        private Vector2 settingsScroll;
        private bool showMipmaps = true;
        private bool showAdvanced = true;
        private bool showSprite;
        private bool showDefaultPlatform = true;
        private bool showAndroid = true;
        private bool showIOS;
        private bool showStandalone;
        private bool showWebGL;

        [MenuItem("Tools/Asset/资源配置设置", false, 40)]
        public static void Open()
        {
            var window = GetWindow<ResourceImportConfigWindow>();
            window.titleContent = new GUIContent("纹理资源配置", EditorGUIUtility.IconContent("Settings").image);
            window.minSize = new Vector2(960, 640);
            window.Show();
        }

        private void OnEnable()
        {
            ResourceImportConfiguration.LoadOrCreate();
        }

        private void OnDisable()
        {
            var config = ResourceImportConfiguration.Instance;
            if (config != null && config.saveOnClose)
            {
                ResourceImportConfiguration.Save(true);
            }
        }

        private static void SaveConfiguration()
        {
            ResourceImportConfiguration.Save(true);
            var window = GetWindow<ResourceImportConfigWindow>(false, null, false);
            window?.ShowNotification(new GUIContent("已保存到 ProjectSettings/ResourceImportConfiguration.asset"));
        }

        private void OnGUI()
        {
            var config = ResourceImportConfiguration.Instance;
            EnsureTemplates(config);

            DrawHeaderToolbar(config);

            EditorGUILayout.Space(4);
            EditorGUILayout.HelpBox("当前仅纹理（图片）。每项左侧勾选才会在导入时写入；未勾选则保留资源原有或你在 Inspector 中的设置。", MessageType.None);
            EditorGUILayout.Space(2);

            EditorGUI.BeginChangeCheck();
            DrawTextureTab(config);
            if (EditorGUI.EndChangeCheck())
            {
                ResourceImportConfiguration.Save(true);
            }
        }

        private static void DrawHeaderToolbar(ResourceImportConfiguration config)
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            if (GUILayout.Button(new GUIContent(" 保存配置", EditorGUIUtility.IconContent("SaveAs").image), EditorStyles.toolbarButton, GUILayout.Width(100)))
            {
                SaveConfiguration();
            }

            config.enableAutoApply = GUILayout.Toggle(config.enableAutoApply, new GUIContent("导入时自动套用", "图片导入或移动时按组目录与词缀规则自动应用"), EditorStyles.toolbarButton);
            config.saveOnClose = GUILayout.Toggle(config.saveOnClose, new GUIContent("关闭窗口时保存", "关闭本窗口时写入 ProjectSettings 配置文件"), EditorStyles.toolbarButton);

            GUILayout.FlexibleSpace();
            if (GUILayout.Button(new GUIContent(" 全部重新导入", EditorGUIUtility.IconContent("Refresh").image), EditorStyles.toolbarButton, GUILayout.Width(120)))
            {
                ResourceImportConfiguration.Save(true);
                ReimportAllTemplates(config);
            }

            EditorGUILayout.EndHorizontal();

            config.pathWhitelist = EditorGUILayout.TextField(
                new GUIContent("路径白名单（逗号分隔）", "路径中包含以下目录名时跳过自动导入与批量重新导入"),
                config.pathWhitelist);
        }

        private void DrawTextureTab(ResourceImportConfiguration config)
        {
            EditorGUILayout.BeginHorizontal();
            DrawGroupList(config);
            DrawGroupMainPanel(config);
            EditorGUILayout.EndHorizontal();
        }

        private void DrawGroupList(ResourceImportConfiguration config)
        {
            EditorGUILayout.BeginVertical("box", GUILayout.Width(200), GUILayout.ExpandHeight(true));
            GUILayout.Label("纹理组", EditorStyles.boldLabel);

            if (GUILayout.Button("增加组", GUILayout.Height(24)))
            {
                Array.Resize(ref config.imageTemplates, config.imageTemplates.Length + 1);
                config.imageTemplates[config.imageTemplates.Length - 1] = ResourceImportConfiguration.CreateDefaultTemplate();
                config.imageTemplates[config.imageTemplates.Length - 1].name = $"新组 {config.imageTemplates.Length}";
                selectedTemplateIndex = config.imageTemplates.Length - 1;
                selectedRuleIndex = -1;
                SaveConfiguration();
            }

            templateListScroll = EditorGUILayout.BeginScrollView(templateListScroll);
            var templates = config.imageTemplates;
            for (int i = 0; i < templates.Length; i++)
            {
                var name = string.IsNullOrWhiteSpace(templates[i].name) ? $"组 {i + 1}" : templates[i].name;
                var selected = i == selectedTemplateIndex;
                var style = selected ? EditorStyles.miniButtonMid : EditorStyles.miniButton;
                if (GUILayout.Toggle(selected, $"{i + 1}: {name}", style, GUILayout.Height(26)) && !selected)
                {
                    selectedTemplateIndex = i;
                    selectedRuleIndex = -1;
                    GUI.FocusControl(null);
                }
            }

            EditorGUILayout.EndScrollView();

            EditorGUILayout.BeginHorizontal();
            using (new EditorGUI.DisabledScope(selectedTemplateIndex <= 0))
            {
                if (GUILayout.Button("▲", GUILayout.Width(28), GUILayout.Height(22)))
                {
                    SwapTemplates(config, selectedTemplateIndex, selectedTemplateIndex - 1);
                    selectedTemplateIndex--;
                    SaveConfiguration();
                }
            }

            using (new EditorGUI.DisabledScope(selectedTemplateIndex >= config.imageTemplates.Length - 1))
            {
                if (GUILayout.Button("▼", GUILayout.Width(28), GUILayout.Height(22)))
                {
                    SwapTemplates(config, selectedTemplateIndex, selectedTemplateIndex + 1);
                    selectedTemplateIndex++;
                    SaveConfiguration();
                }
            }

            using (new EditorGUI.DisabledScope(config.imageTemplates.Length <= 1))
            {
                if (GUILayout.Button("删除组", GUILayout.Height(22)))
                {
                    if (EditorUtility.DisplayDialog("删除组", $"确定删除「{GetSelectedTemplate(config)?.name}」？", "删除", "取消"))
                    {
                        RemoveSelectedTemplate(config);
                        SaveConfiguration();
                    }
                }
            }

            EditorGUILayout.EndHorizontal();

            if (GUILayout.Button("复制当前组", GUILayout.Height(22)))
            {
                CopySelectedTemplate(config);
                SaveConfiguration();
            }

            var group = GetSelectedTemplate(config);
            using (new EditorGUI.DisabledScope(group == null))
            {
                if (GUILayout.Button(new GUIContent(" 重新导入当前组", EditorGUIUtility.IconContent("Refresh").image), GUILayout.Height(28)))
                {
                    ResourceImportConfiguration.Save(true);
                    ResourceImportApplier.ReimportGroup(group, "重新导入当前组");
                }
            }

            EditorGUILayout.EndVertical();
        }

        private void DrawGroupMainPanel(ResourceImportConfiguration config)
        {
            var template = GetSelectedTemplate(config);
            EditorGUILayout.BeginVertical("box", GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
            if (template == null)
            {
                EditorGUILayout.HelpBox("请先增加一个纹理组。", MessageType.Warning);
                EditorGUILayout.EndVertical();
                return;
            }

            settingsScroll = EditorGUILayout.BeginScrollView(settingsScroll);

            template.name = EditorGUILayout.TextField("组名称", template.name);
            DrawOverlapWarning(config, template);

            EditorGUILayout.Space(4);
            GUILayout.Label("纹理导入设置", EditorStyles.boldLabel);
            template.applyMask ??= new ImageImportFieldApplyMask();
            if (!template.applyMask.HasAny())
            {
                EditorGUILayout.HelpBox("本组尚未勾选任何导入项，匹配到此目录的图片不会被本工具改写。", MessageType.Warning);
            }

            DrawTemplateImportSettings(template, template.applyMask);

            EditorGUILayout.Space(8);
            DrawFolderListSection("组策略目录", ref template.folders, true, template);
            DrawFolderListSection("组策略忽略目录", ref template.ignoreFolders, false, template);
            DrawSpecialRules(config, template);

            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
        }

        private void DrawTemplateImportSettings(ImageImportTemplate editSource, ImageImportFieldApplyMask mask)
        {
            mask ??= new ImageImportFieldApplyMask();

            DrawApplyEnum("Texture Type", ref mask.textureType, ref editSource.textureType);
            DrawApplyEnum("Texture Shape", ref mask.textureShape, ref editSource.textureShape);

            showAdvanced = EditorGUILayout.BeginFoldoutHeaderGroup(showAdvanced, "高级");
            if (showAdvanced)
            {
                EditorGUILayout.BeginVertical("box");
                DrawApplyToggle("sRGB (Color Texture)", ref mask.sRGBTexture, ref editSource.sRGBTexture);
                DrawApplyEnum("Alpha Source", ref mask.alphaSource, ref editSource.alphaSource);
                DrawApplyToggle("Alpha Is Transparency", ref mask.alphaIsTransparency, ref editSource.alphaIsTransparency);
                DrawApplyEnum("Non-Power of 2", ref mask.npotScale, ref editSource.npotScale);
                DrawApplyToggle("Read/Write", ref mask.isReadable, ref editSource.isReadable);
                DrawApplyToggle("Streaming Mipmaps", ref mask.streamingMipmaps, ref editSource.streamingMipmaps);
                EditorGUILayout.EndVertical();
            }

            EditorGUILayout.EndFoldoutHeaderGroup();

            showMipmaps = EditorGUILayout.BeginFoldoutHeaderGroup(showMipmaps, "Mipmaps");
            if (showMipmaps)
            {
                EditorGUILayout.BeginVertical("box");
                DrawApplyToggle("Generate Mip Maps", ref mask.mipmapEnabled, ref editSource.mipmapEnabled);
                DrawApplyToggle("Border Mip Maps", ref mask.borderMipmap, ref editSource.borderMipmap, !editSource.mipmapEnabled);
                DrawApplyToggle("Mip Maps Preserve Coverage", ref mask.mipMapsPreserveCoverage, ref editSource.mipMapsPreserveCoverage, !editSource.mipmapEnabled);
                DrawApplyToggle("Fadeout Mip Maps", ref mask.fadeout, ref editSource.fadeout, !editSource.mipmapEnabled);
                EditorGUILayout.EndVertical();
            }

            EditorGUILayout.EndFoldoutHeaderGroup();

            showSprite = EditorGUILayout.BeginFoldoutHeaderGroup(showSprite, "Sprite");
            if (showSprite)
            {
                EditorGUILayout.BeginVertical("box");
                DrawApplyEnum("Sprite Mode", ref mask.spriteImportMode, ref editSource.spriteImportMode);
                DrawApplyFloat("Pixels Per Unit", ref mask.spritePixelsPerUnit, ref editSource.spritePixelsPerUnit);
                DrawApplyEnum("Mesh Type", ref mask.spriteMeshType, ref editSource.spriteMeshType);
                DrawApplyUInt("Extrude Edges", ref mask.spriteExtrude, ref editSource.spriteExtrude);
                EditorGUILayout.EndVertical();
            }

            EditorGUILayout.EndFoldoutHeaderGroup();

            EditorGUILayout.Space(2);
            GUILayout.Label("过滤与环绕", EditorStyles.boldLabel);
            EditorGUILayout.BeginVertical("box");
            DrawApplyEnum("Filter Mode", ref mask.filterMode, ref editSource.filterMode);
            DrawApplyEnum("Wrap Mode", ref mask.wrapMode, ref editSource.wrapMode);
            DrawApplyIntSlider("Aniso Level", ref mask.anisoLevel, ref editSource.anisoLevel, 0, 16);
            EditorGUILayout.EndVertical();

            showDefaultPlatform = EditorGUILayout.BeginFoldoutHeaderGroup(showDefaultPlatform, "压缩 / Default 平台");
            if (showDefaultPlatform)
            {
                EditorGUILayout.BeginVertical("box");
                DrawApplyMaxSize("Max Size", ref mask.maxTextureSize, ref editSource.maxTextureSize);
                DrawApplyEnum("Compression", ref mask.textureCompression, ref editSource.textureCompression);
                DrawApplyToggle("Use Crunch Compression", ref mask.crunchedCompression, ref editSource.crunchedCompression);
                DrawApplyIntSlider("Compressor Quality", ref mask.compressionQuality, ref editSource.compressionQuality, 0, 100);
                EditorGUILayout.EndVertical();
            }

            EditorGUILayout.EndFoldoutHeaderGroup();

            DrawPlatformFoldout("Android 覆盖", ref showAndroid, ref mask.android, editSource.android);
            DrawPlatformFoldout("iOS 覆盖", ref showIOS, ref mask.iOS, editSource.iOS);
            DrawPlatformFoldout("Standalone 覆盖", ref showStandalone, ref mask.standalone, editSource.standalone);
            DrawPlatformFoldout("WebGL 覆盖", ref showWebGL, ref mask.webGL, editSource.webGL);
        }

        private static void DrawApplyToggle(string label, ref bool apply, ref bool value, bool forceDisabled = false)
        {
            EditorGUILayout.BeginHorizontal();
            apply = EditorGUILayout.Toggle(apply, GUILayout.Width(18));
            using (new EditorGUI.DisabledScope(!apply || forceDisabled))
            {
                value = EditorGUILayout.Toggle(label, value);
            }

            EditorGUILayout.EndHorizontal();
        }

        private static void DrawApplyEnum<T>(string label, ref bool apply, ref T value) where T : Enum
        {
            EditorGUILayout.BeginHorizontal();
            apply = EditorGUILayout.Toggle(apply, GUILayout.Width(18));
            using (new EditorGUI.DisabledScope(!apply))
            {
                value = (T)EditorGUILayout.EnumPopup(label, value);
            }

            EditorGUILayout.EndHorizontal();
        }

        private static void DrawApplyFloat(string label, ref bool apply, ref float value)
        {
            EditorGUILayout.BeginHorizontal();
            apply = EditorGUILayout.Toggle(apply, GUILayout.Width(18));
            using (new EditorGUI.DisabledScope(!apply))
            {
                value = EditorGUILayout.FloatField(label, value);
            }

            EditorGUILayout.EndHorizontal();
        }

        private static void DrawApplyUInt(string label, ref bool apply, ref uint value)
        {
            EditorGUILayout.BeginHorizontal();
            apply = EditorGUILayout.Toggle(apply, GUILayout.Width(18));
            using (new EditorGUI.DisabledScope(!apply))
            {
                value = (uint)Mathf.Max(0, EditorGUILayout.IntField(label, (int)value));
            }

            EditorGUILayout.EndHorizontal();
        }

        private static void DrawApplyIntSlider(string label, ref bool apply, ref int value, int min, int max)
        {
            EditorGUILayout.BeginHorizontal();
            apply = EditorGUILayout.Toggle(apply, GUILayout.Width(18));
            using (new EditorGUI.DisabledScope(!apply))
            {
                value = EditorGUILayout.IntSlider(label, value, min, max);
            }

            EditorGUILayout.EndHorizontal();
        }

        private static void DrawApplyMaxSize(string label, ref bool apply, ref int value)
        {
            EditorGUILayout.BeginHorizontal();
            apply = EditorGUILayout.Toggle(apply, GUILayout.Width(18));
            using (new EditorGUI.DisabledScope(!apply))
            {
                value = DrawMaxSize(label, value);
            }

            EditorGUILayout.EndHorizontal();
        }

        private void DrawFolderListSection(string title, ref string[] list, bool showReimport, ImageImportTemplate group)
        {
            list ??= Array.Empty<string>();
            GUILayout.Label(title, EditorStyles.boldLabel);
            EditorGUILayout.BeginVertical("box");
            if (list.Length == 0)
            {
                EditorGUILayout.LabelField("（空）");
            }

            int removeIndex = -1;
            for (int i = 0; i < list.Length; i++)
            {
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("X", GUILayout.Width(22)))
                {
                    removeIndex = i;
                }

                GUILayout.Label($"{i + 1}:", GUILayout.Width(22));
                list[i] = DrawFolderFieldInline(list[i]);
                using (new EditorGUI.DisabledScope(i <= 0))
                {
                    if (GUILayout.Button("▲", GUILayout.Width(24)))
                    {
                        (list[i], list[i - 1]) = (list[i - 1], list[i]);
                    }
                }

                using (new EditorGUI.DisabledScope(i >= list.Length - 1))
                {
                    if (GUILayout.Button("▼", GUILayout.Width(24)))
                    {
                        (list[i], list[i + 1]) = (list[i + 1], list[i]);
                    }
                }

                if (showReimport)
                {
                    if (GUILayout.Button("重新导入", GUILayout.Width(72)))
                    {
                        ResourceImportConfiguration.Save(true);
                        ResourceImportApplier.ReimportFolders(new[] { list[i] }, $"重新导入 {list[i]}", group);
                    }
                }

                EditorGUILayout.EndHorizontal();
                ValidateFolderPath(list[i]);
            }

            if (removeIndex >= 0)
            {
                ArrayUtility.RemoveAt(ref list, removeIndex);
            }

            if (GUILayout.Button(showReimport ? "增加目录" : "增加忽略目录", GUILayout.Height(22)))
            {
                Array.Resize(ref list, list.Length + 1);
                list[list.Length - 1] = "Assets";
            }

            EditorGUILayout.EndVertical();
        }

        private void DrawSpecialRules(ResourceImportConfiguration config, ImageImportTemplate group)
        {
            if (group.specialRules == null)
            {
                group.specialRules = Array.Empty<ImageImportSpecialRule>();
            }

            GUILayout.Label("特殊词缀匹配", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("自上而下匹配；词缀规则中勾选的项会覆盖组默认里对应项。", MessageType.None);

            EditorGUILayout.BeginVertical("box");
            int removeRule = -1;
            for (int i = 0; i < group.specialRules.Length; i++)
            {
                var rule = group.specialRules[i];
                rule.applyMask ??= new ImageImportFieldApplyMask();
                if (rule.settings == null)
                {
                    rule.settings = ResourceImportConfiguration.CreateRuleSettingsStub();
                }

                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("X", GUILayout.Width(22)))
                {
                    removeRule = i;
                }

                GUILayout.Label($"{i + 1}:", GUILayout.Width(22));
                rule.matchKind = (ImportPathMatchKind)EditorGUILayout.EnumPopup(rule.matchKind, GUILayout.Width(180));
                rule.pattern = EditorGUILayout.TextField(rule.pattern);

                using (new EditorGUI.DisabledScope(i <= 0))
                {
                    if (GUILayout.Button("▲", GUILayout.Width(24)))
                    {
                        (group.specialRules[i], group.specialRules[i - 1]) = (group.specialRules[i - 1], group.specialRules[i]);
                    }
                }

                using (new EditorGUI.DisabledScope(i >= group.specialRules.Length - 1))
                {
                    if (GUILayout.Button("▼", GUILayout.Width(24)))
                    {
                        (group.specialRules[i], group.specialRules[i + 1]) = (group.specialRules[i + 1], group.specialRules[i]);
                    }
                }

                if (GUILayout.Button(selectedRuleIndex == i ? "收起" : "编辑", GUILayout.Width(44)))
                {
                    selectedRuleIndex = selectedRuleIndex == i ? -1 : i;
                }

                EditorGUILayout.EndHorizontal();

                if (rule.matchKind == ImportPathMatchKind.PathRegex && !string.IsNullOrEmpty(rule.pattern))
                {
                    try
                    {
                        System.Text.RegularExpressions.Regex.IsMatch("Assets/test.png", rule.pattern);
                    }
                    catch (ArgumentException ex)
                    {
                        EditorGUILayout.HelpBox($"正则无效: {ex.Message}", MessageType.Warning);
                    }
                }

                if (selectedRuleIndex == i)
                {
                    EditorGUILayout.BeginVertical("helpbox");
                    if (!rule.applyMask.HasAny())
                    {
                        EditorGUILayout.HelpBox("请勾选需要由本条词缀改写的导入项。", MessageType.Warning);
                    }

                    DrawTemplateImportSettings(rule.settings, rule.applyMask);
                    EditorGUILayout.EndVertical();
                }
            }

            if (removeRule >= 0)
            {
                ArrayUtility.RemoveAt(ref group.specialRules, removeRule);
                selectedRuleIndex = -1;
            }

            if (GUILayout.Button("增加词缀匹配", GUILayout.Height(24)))
            {
                Array.Resize(ref group.specialRules, group.specialRules.Length + 1);
                group.specialRules[group.specialRules.Length - 1] = ResourceImportConfiguration.CreateDefaultSpecialRule();
                selectedRuleIndex = group.specialRules.Length - 1;
            }

            EditorGUILayout.EndVertical();
        }

        private static void ValidateFolderPath(string path)
        {
            var normalized = ResourceImportApplier.NormalizeAssetFolder(path);
            if (!string.IsNullOrEmpty(normalized) && !ResourceImportApplier.IsValidAssetsFolder(normalized))
            {
                EditorGUILayout.HelpBox($"「{path}」必须以 Assets 开头。", MessageType.Error);
            }
            else if (!string.IsNullOrEmpty(normalized) && !AssetDatabase.IsValidFolder(normalized))
            {
                EditorGUILayout.HelpBox($"「{path}」在工程中不存在。", MessageType.Warning);
            }
        }

        private static void DrawOverlapWarning(ResourceImportConfiguration config, ImageImportTemplate current)
        {
            if (current.folders == null)
            {
                return;
            }

            var overlaps = new List<string>();
            foreach (var folder in current.folders)
            {
                var normalized = ResourceImportApplier.NormalizeAssetFolder(folder);
                if (!ResourceImportApplier.IsValidAssetsFolder(normalized))
                {
                    continue;
                }

                foreach (var other in config.imageTemplates)
                {
                    if (other == null || other == current || other.folders == null)
                    {
                        continue;
                    }

                    foreach (var otherFolder in other.folders)
                    {
                        var otherNormalized = ResourceImportApplier.NormalizeAssetFolder(otherFolder);
                        if (!ResourceImportApplier.IsValidAssetsFolder(otherNormalized))
                        {
                            continue;
                        }

                        if (normalized == otherNormalized
                            || ResourceImportApplier.IsUnderFolder(normalized, otherNormalized)
                            || ResourceImportApplier.IsUnderFolder(otherNormalized, normalized))
                        {
                            overlaps.Add($"{normalized} 与组「{other.name}」的 {otherNormalized} 存在包含关系，导入时按最长路径匹配。");
                        }
                    }
                }
            }

            if (overlaps.Count > 0)
            {
                EditorGUILayout.HelpBox(string.Join("\n", overlaps), MessageType.Warning);
            }
        }

        private static void DrawPlatformFoldout(string title, ref bool foldout, ref bool apply, ImagePlatformOverride settings)
        {
            settings ??= new ImagePlatformOverride();
            EditorGUILayout.BeginHorizontal();
            apply = EditorGUILayout.Toggle(apply, GUILayout.Width(18));
            foldout = EditorGUILayout.Foldout(foldout, title, true);
            EditorGUILayout.EndHorizontal();

            if (foldout)
            {
                using (new EditorGUI.DisabledScope(!apply))
                {
                    EditorGUILayout.BeginVertical("box");
                    settings.overridden = EditorGUILayout.Toggle("Override", settings.overridden);
                    using (new EditorGUI.DisabledScope(!settings.overridden))
                    {
                        settings.maxTextureSize = DrawMaxSize("Max Size", settings.maxTextureSize);
                        settings.format = (TextureImporterFormat)EditorGUILayout.EnumPopup("Format", settings.format);
                        settings.compressionQuality = EditorGUILayout.IntSlider("Compressor Quality", settings.compressionQuality, 0, 100);
                    }

                    EditorGUILayout.EndVertical();
                }
            }
        }

        private static int DrawMaxSize(string label, int value)
        {
            int index = Array.IndexOf(MaxSizeOptions, value);
            if (index < 0)
            {
                index = 6;
            }

            var labels = new string[MaxSizeOptions.Length];
            for (int i = 0; i < MaxSizeOptions.Length; i++)
            {
                labels[i] = MaxSizeOptions[i].ToString();
            }

            return EditorGUILayout.IntPopup(label, MaxSizeOptions[index], labels, MaxSizeOptions);
        }

        private static string DrawFolderFieldInline(string path)
        {
            EditorGUILayout.BeginHorizontal();
            path = EditorGUILayout.TextField(path);
            if (GUILayout.Button("选", GUILayout.Width(28)))
            {
                var picked = EditorUtility.OpenFolderPanel("选择文件夹", Application.dataPath, string.Empty);
                if (!string.IsNullOrEmpty(picked))
                {
                    var dataPath = Application.dataPath.Replace('\\', '/');
                    picked = picked.Replace('\\', '/');
                    if (picked.StartsWith(dataPath, StringComparison.OrdinalIgnoreCase))
                    {
                        path = "Assets" + picked.Substring(dataPath.Length);
                    }
                    else
                    {
                        EditorUtility.DisplayDialog("无效路径", "只能选择工程内以 Assets 开头的文件夹。", "确定");
                    }
                }
            }

            EditorGUILayout.EndHorizontal();
            return path;
        }

        private ImageImportTemplate GetSelectedTemplate(ResourceImportConfiguration config)
        {
            if (config.imageTemplates == null || config.imageTemplates.Length == 0)
            {
                return null;
            }

            selectedTemplateIndex = Mathf.Clamp(selectedTemplateIndex, 0, config.imageTemplates.Length - 1);
            return config.imageTemplates[selectedTemplateIndex];
        }

        private static void EnsureTemplates(ResourceImportConfiguration config)
        {
            if (config.imageTemplates != null && config.imageTemplates.Length > 0)
            {
                foreach (var t in config.imageTemplates)
                {
                    if (t == null)
                    {
                        continue;
                    }

                    ResourceImportConfiguration.MigrateApplyMask(t);
                    t.applyMask ??= new ImageImportFieldApplyMask();
                    t.folders ??= Array.Empty<string>();
                    t.ignoreFolders ??= Array.Empty<string>();
                    t.specialRules ??= Array.Empty<ImageImportSpecialRule>();
                    foreach (var rule in t.specialRules)
                    {
                        if (rule == null)
                        {
                            continue;
                        }

                        rule.applyMask ??= new ImageImportFieldApplyMask();
                        rule.settings ??= ResourceImportConfiguration.CreateRuleSettingsStub();
                    }
                }

                return;
            }

            config.imageTemplates = new[] { ResourceImportConfiguration.CreateDefaultTemplate() };
            SaveConfiguration();
        }

        private void RemoveSelectedTemplate(ResourceImportConfiguration config)
        {
            var list = new List<ImageImportTemplate>(config.imageTemplates);
            if (selectedTemplateIndex < 0 || selectedTemplateIndex >= list.Count)
            {
                return;
            }

            list.RemoveAt(selectedTemplateIndex);
            config.imageTemplates = list.ToArray();
            selectedTemplateIndex = Mathf.Clamp(selectedTemplateIndex, 0, config.imageTemplates.Length - 1);
            selectedRuleIndex = -1;
        }

        private void CopySelectedTemplate(ResourceImportConfiguration config)
        {
            var source = GetSelectedTemplate(config);
            if (source == null)
            {
                return;
            }

            var copy = ResourceImportApplier.CloneTemplate(source);
            copy.name = source.name + " 副本";
            Array.Resize(ref config.imageTemplates, config.imageTemplates.Length + 1);
            config.imageTemplates[config.imageTemplates.Length - 1] = copy;
            selectedTemplateIndex = config.imageTemplates.Length - 1;
            selectedRuleIndex = -1;
        }

        private static void SwapTemplates(ResourceImportConfiguration config, int a, int b)
        {
            (config.imageTemplates[a], config.imageTemplates[b]) = (config.imageTemplates[b], config.imageTemplates[a]);
        }

        private static void ReimportAllTemplates(ResourceImportConfiguration config)
        {
            var folders = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var template in config.imageTemplates)
            {
                if (template?.folders == null)
                {
                    continue;
                }

                foreach (var folder in template.folders)
                {
                    folders.Add(folder);
                }
            }

            ResourceImportApplier.ReimportFolders(folders, "全部重新导入");
        }
    }
}