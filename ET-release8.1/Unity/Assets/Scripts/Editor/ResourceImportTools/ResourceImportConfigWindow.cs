using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace ET
{
    public class ResourceImportConfigWindow : EditorWindow
    {
        private static readonly string[] ResourceTypeTabs = { "图片" };
        private static readonly int[] MaxSizeOptions = { 32, 64, 128, 256, 512, 1024, 2048, 4096, 8192 };

        private int resourceTypeTab;
        private int selectedTemplateIndex;
        private Vector2 templateListScroll;
        private Vector2 settingsScroll;
        private bool showFolders = true;
        private bool showAdvanced = true;
        private bool showMipmaps = true;
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
            window.titleContent = new GUIContent("资源配置设置", EditorGUIUtility.IconContent("Settings").image);
            window.minSize = new Vector2(860, 560);
            window.Show();
        }

        private void OnEnable()
        {
            ResourceImportConfiguration.LoadOrCreate();
        }

        private void OnDisable()
        {
            ResourceImportConfiguration.Save(true);
        }

        private static void SaveConfiguration()
        {
            ResourceImportConfiguration.Save(true);
        }

        private void OnGUI()
        {
            var config = ResourceImportConfiguration.Instance;
            EnsureTemplates(config);

            EditorGUILayout.Space(6);
            resourceTypeTab = GUILayout.Toolbar(resourceTypeTab, ResourceTypeTabs, GUILayout.Height(24));
            EditorGUILayout.Space(6);

            EditorGUI.BeginChangeCheck();
            DrawImageTab(config);
            if (EditorGUI.EndChangeCheck())
            {
                SaveConfiguration();
            }
        }

        private void DrawImageTab(ResourceImportConfiguration config)
        {
            EditorGUILayout.BeginHorizontal();
            config.enableAutoApply = EditorGUILayout.ToggleLeft(
                new GUIContent("导入时按所在文件夹自动套用模板", "单张图片导入或移动后，按最长匹配的 Assets 文件夹应用对应模板。"),
                config.enableAutoApply);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.HelpBox(
                "左侧维护图片模板。每个模板可指定多个以 Assets 开头的文件夹。点击导入会按该模板重新导入对应目录下的全部图片。",
                MessageType.Info);

            EditorGUILayout.BeginHorizontal();
            DrawTemplateList(config);
            DrawTemplateSettings(config);
            EditorGUILayout.EndHorizontal();
        }

        private void DrawTemplateList(ResourceImportConfiguration config)
        {
            EditorGUILayout.BeginVertical("box", GUILayout.Width(200), GUILayout.ExpandHeight(true));
            GUILayout.Label("图片模板", EditorStyles.boldLabel);

            templateListScroll = EditorGUILayout.BeginScrollView(templateListScroll);
            var templates = config.imageTemplates;
            for (int i = 0; i < templates.Length; i++)
            {
                var name = string.IsNullOrWhiteSpace(templates[i].name) ? $"模板 {i + 1}" : templates[i].name;
                var selected = i == selectedTemplateIndex;
                var style = selected ? EditorStyles.miniButtonMid : EditorStyles.miniButton;
                if (GUILayout.Toggle(selected, name, style, GUILayout.Height(26)) && !selected)
                {
                    selectedTemplateIndex = i;
                    GUI.FocusControl(null);
                }
            }

            EditorGUILayout.EndScrollView();

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button(new GUIContent("新增", "新增一个图片模板"), GUILayout.Height(24)))
            {
                Array.Resize(ref config.imageTemplates, config.imageTemplates.Length + 1);
                config.imageTemplates[config.imageTemplates.Length - 1] = ResourceImportConfiguration.CreateDefaultTemplate();
                config.imageTemplates[config.imageTemplates.Length - 1].name = $"新模板 {config.imageTemplates.Length}";
                selectedTemplateIndex = config.imageTemplates.Length - 1;
                SaveConfiguration();
            }

            using (new EditorGUI.DisabledScope(config.imageTemplates.Length <= 1))
            {
                if (GUILayout.Button(new GUIContent("删除", "删除当前模板"), GUILayout.Height(24)))
                {
                    if (EditorUtility.DisplayDialog("删除模板", $"确定删除模板“{GetSelectedTemplate(config)?.name}”？", "删除", "取消"))
                    {
                        RemoveSelectedTemplate(config);
                        SaveConfiguration();
                    }
                }
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            using (new EditorGUI.DisabledScope(selectedTemplateIndex <= 0))
            {
                if (GUILayout.Button("上移", GUILayout.Height(22)))
                {
                    SwapTemplates(config, selectedTemplateIndex, selectedTemplateIndex - 1);
                    selectedTemplateIndex--;
                    SaveConfiguration();
                }
            }

            using (new EditorGUI.DisabledScope(selectedTemplateIndex >= config.imageTemplates.Length - 1))
            {
                if (GUILayout.Button("下移", GUILayout.Height(22)))
                {
                    SwapTemplates(config, selectedTemplateIndex, selectedTemplateIndex + 1);
                    selectedTemplateIndex++;
                    SaveConfiguration();
                }
            }

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
        }

        private void DrawTemplateSettings(ResourceImportConfiguration config)
        {
            var template = GetSelectedTemplate(config);
            EditorGUILayout.BeginVertical("box", GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
            if (template == null)
            {
                EditorGUILayout.HelpBox("请先新增一个图片模板。", MessageType.Warning);
                EditorGUILayout.EndVertical();
                return;
            }

            settingsScroll = EditorGUILayout.BeginScrollView(settingsScroll);

            GUILayout.Label("模板设置", EditorStyles.boldLabel);
            template.name = EditorGUILayout.TextField("模板名称", template.name);
            DrawFolderList(template);
            DrawOverlapWarning(config, template);

            EditorGUILayout.Space(4);
            GUILayout.Label("纹理类型", EditorStyles.boldLabel);
            template.textureType = (TextureImporterType)EditorGUILayout.EnumPopup("Texture Type", template.textureType);
            template.textureShape = (TextureImporterShape)EditorGUILayout.EnumPopup("Texture Shape", template.textureShape);

            showAdvanced = EditorGUILayout.BeginFoldoutHeaderGroup(showAdvanced, "高级");
            if (showAdvanced)
            {
                EditorGUILayout.BeginVertical("box");
                template.sRGBTexture = EditorGUILayout.Toggle("sRGB (Color Texture)", template.sRGBTexture);
                template.alphaSource = (TextureImporterAlphaSource)EditorGUILayout.EnumPopup("Alpha Source", template.alphaSource);
                template.alphaIsTransparency = EditorGUILayout.Toggle("Alpha Is Transparency", template.alphaIsTransparency);
                template.npotScale = (TextureImporterNPOTScale)EditorGUILayout.EnumPopup("Non-Power of 2", template.npotScale);
                template.isReadable = EditorGUILayout.Toggle("Read/Write", template.isReadable);
                template.streamingMipmaps = EditorGUILayout.Toggle("Streaming Mipmaps", template.streamingMipmaps);
                EditorGUILayout.EndVertical();
            }

            EditorGUILayout.EndFoldoutHeaderGroup();

            showMipmaps = EditorGUILayout.BeginFoldoutHeaderGroup(showMipmaps, "Mipmaps");
            if (showMipmaps)
            {
                EditorGUILayout.BeginVertical("box");
                template.mipmapEnabled = EditorGUILayout.Toggle("Generate Mip Maps", template.mipmapEnabled);
                using (new EditorGUI.DisabledScope(!template.mipmapEnabled))
                {
                    template.borderMipmap = EditorGUILayout.Toggle("Border Mip Maps", template.borderMipmap);
                    template.mipMapsPreserveCoverage = EditorGUILayout.Toggle("Mip Maps Preserve Coverage", template.mipMapsPreserveCoverage);
                    template.fadeout = EditorGUILayout.Toggle("Fadeout Mip Maps", template.fadeout);
                }

                EditorGUILayout.EndVertical();
            }

            EditorGUILayout.EndFoldoutHeaderGroup();

            if (template.textureType == TextureImporterType.Sprite)
            {
                showSprite = EditorGUILayout.BeginFoldoutHeaderGroup(showSprite, "Sprite");
                if (showSprite)
                {
                    EditorGUILayout.BeginVertical("box");
                    template.spriteImportMode = (SpriteImportMode)EditorGUILayout.EnumPopup("Sprite Mode", template.spriteImportMode);
                    template.spritePixelsPerUnit = EditorGUILayout.FloatField("Pixels Per Unit", template.spritePixelsPerUnit);
                    template.spriteMeshType = (SpriteMeshType)EditorGUILayout.EnumPopup("Mesh Type", template.spriteMeshType);
                    template.spriteExtrude = (uint)Mathf.Max(0, EditorGUILayout.IntField("Extrude Edges", (int)template.spriteExtrude));
                    EditorGUILayout.EndVertical();
                }

                EditorGUILayout.EndFoldoutHeaderGroup();
            }

            EditorGUILayout.Space(4);
            GUILayout.Label("默认", EditorStyles.boldLabel);
            EditorGUILayout.BeginVertical("box");
            template.filterMode = (FilterMode)EditorGUILayout.EnumPopup("Filter Mode", template.filterMode);
            template.wrapMode = (TextureWrapMode)EditorGUILayout.EnumPopup("Wrap Mode", template.wrapMode);
            template.anisoLevel = EditorGUILayout.IntSlider("Aniso Level", template.anisoLevel, 0, 16);
            EditorGUILayout.EndVertical();

            showDefaultPlatform = EditorGUILayout.BeginFoldoutHeaderGroup(showDefaultPlatform, "Default 平台");
            if (showDefaultPlatform)
            {
                EditorGUILayout.BeginVertical("box");
                template.maxTextureSize = DrawMaxSize("Max Size", template.maxTextureSize);
                template.textureCompression = (TextureImporterCompression)EditorGUILayout.EnumPopup("Compression", template.textureCompression);
                template.crunchedCompression = EditorGUILayout.Toggle("Use Crunch Compression", template.crunchedCompression);
                template.compressionQuality = EditorGUILayout.IntSlider("Compressor Quality", template.compressionQuality, 0, 100);
                EditorGUILayout.EndVertical();
            }

            EditorGUILayout.EndFoldoutHeaderGroup();

            DrawPlatformFoldout("Android 覆盖", ref showAndroid, template.android);
            DrawPlatformFoldout("iOS 覆盖", ref showIOS, template.iOS);
            DrawPlatformFoldout("Standalone 覆盖", ref showStandalone, template.standalone);
            DrawPlatformFoldout("WebGL 覆盖", ref showWebGL, template.webGL);

            EditorGUILayout.EndScrollView();

            EditorGUILayout.Space(6);
            using (new EditorGUILayout.HorizontalScope())
            {
                var original = GUI.color;
                GUI.color = new Color(0.55f, 0.85f, 0.55f);
                if (GUILayout.Button(new GUIContent(" 导入当前模板", EditorGUIUtility.IconContent("Refresh").image), GUILayout.Height(32)))
                {
                    ResourceImportConfiguration.Save(true);
                    ResourceImportApplier.ReimportFolders(template.folders, "导入当前模板", template);
                }

                GUI.color = original;
                if (GUILayout.Button(new GUIContent(" 导入全部模板", EditorGUIUtility.IconContent("Refresh").image), GUILayout.Height(32)))
                {
                    ResourceImportConfiguration.Save(true);
                    ReimportAllTemplates(config);
                }
            }

            EditorGUILayout.EndVertical();
        }

        private void DrawFolderList(ImageImportTemplate template)
        {
            EditorGUILayout.BeginHorizontal();
            showFolders = EditorGUILayout.BeginFoldoutHeaderGroup(showFolders, "应用文件夹（必须以 Assets 开头）");
            if (showFolders)
            {
                GUILayout.Label("数量:", GUILayout.ExpandWidth(false));
                int newSize = Mathf.Max(0, EditorGUILayout.IntField(template.folders.Length, GUILayout.Width(40)));
                if (newSize != template.folders.Length)
                {
                    Array.Resize(ref template.folders, newSize);
                    SaveConfiguration();
                }

                if (GUILayout.Button(EditorGUIUtility.IconContent("Toolbar Plus"), GUILayout.Width(25), GUILayout.Height(20)))
                {
                    Array.Resize(ref template.folders, template.folders.Length + 1);
                    template.folders[template.folders.Length - 1] = "Assets";
                    SaveConfiguration();
                }

                using (new EditorGUI.DisabledScope(template.folders.Length == 0))
                {
                    if (GUILayout.Button(EditorGUIUtility.IconContent("Toolbar Minus"), GUILayout.Width(25), GUILayout.Height(20)))
                    {
                        Array.Resize(ref template.folders, template.folders.Length - 1);
                        SaveConfiguration();
                    }
                }
            }

            EditorGUILayout.EndHorizontal();

            if (showFolders)
            {
                EditorGUILayout.BeginVertical("box");
                if (template.folders.Length == 0)
                {
                    EditorGUILayout.HelpBox("尚未指定文件夹。导入不会处理任何图片。", MessageType.Warning);
                }

                for (int i = 0; i < template.folders.Length; i++)
                {
                    template.folders[i] = DrawFolderField($"文件夹 [{i}]", template.folders[i]);
                    var normalized = ResourceImportApplier.NormalizeAssetFolder(template.folders[i]);
                    if (!string.IsNullOrEmpty(normalized) && !ResourceImportApplier.IsValidAssetsFolder(normalized))
                    {
                        EditorGUILayout.HelpBox("路径必须以 Assets 开头。", MessageType.Error);
                    }
                    else if (!string.IsNullOrEmpty(normalized) && !AssetDatabase.IsValidFolder(normalized))
                    {
                        EditorGUILayout.HelpBox("该文件夹在工程中不存在。", MessageType.Warning);
                    }
                }

                EditorGUILayout.EndVertical();
            }

            EditorGUILayout.EndFoldoutHeaderGroup();
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
                            overlaps.Add($"{normalized} 与模板“{other.name}”的 {otherNormalized} 存在包含关系，导入时按最长路径匹配。");
                        }
                    }
                }
            }

            if (overlaps.Count > 0)
            {
                EditorGUILayout.HelpBox(string.Join("\n", overlaps), MessageType.Warning);
            }
        }

        private static void DrawPlatformFoldout(string title, ref bool foldout, ImagePlatformOverride settings)
        {
            foldout = EditorGUILayout.BeginFoldoutHeaderGroup(foldout, title);
            if (foldout)
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

            EditorGUILayout.EndFoldoutHeaderGroup();
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

        private static string DrawFolderField(string label, string path)
        {
            EditorGUILayout.BeginHorizontal();
            path = EditorGUILayout.TextField(new GUIContent(label, EditorGUIUtility.IconContent("Folder Icon").image), path);
            if (GUILayout.Button("选择", GUILayout.Width(56), GUILayout.Height(18)))
            {
                var startPath = Application.dataPath;
                var picked = EditorUtility.OpenFolderPanel(label, startPath, string.Empty);
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
        }

        private static void SwapTemplates(ResourceImportConfiguration config, int a, int b)
        {
            (config.imageTemplates[a], config.imageTemplates[b]) = (config.imageTemplates[b], config.imageTemplates[a]);
        }

        private static void ReimportAllTemplates(ResourceImportConfiguration config)
        {
            var folders = new List<string>();
            foreach (var template in config.imageTemplates)
            {
                if (template?.folders == null)
                {
                    continue;
                }

                folders.AddRange(template.folders);
            }

            ResourceImportApplier.ReimportFolders(folders, "导入全部模板");
        }
    }
}
