using System;
using System.Collections.Generic;
using System.IO;
using TEngine.Editor;
using UnityEditor;
using UnityEngine;

namespace ET
{
    public static class ResourceImportApplier
    {
        public static readonly string[] ImageExtensions =
        {
            ".png", ".jpg", ".jpeg", ".tga", ".bmp", ".psd", ".tif", ".tiff", ".gif", ".exr", ".hdr"
        };

        public static bool IsImageAsset(string assetPath)
        {
            if (string.IsNullOrEmpty(assetPath))
            {
                return false;
            }

            var ext = Path.GetExtension(assetPath);
            for (int i = 0; i < ImageExtensions.Length; i++)
            {
                if (ext.Equals(ImageExtensions[i], StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        public static string NormalizeAssetFolder(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return string.Empty;
            }

            return path.Replace('\\', '/').Trim().TrimEnd('/');
        }

        public static bool IsValidAssetsFolder(string path)
        {
            var normalized = NormalizeAssetFolder(path);
            return normalized == "Assets" || normalized.StartsWith("Assets/", StringComparison.Ordinal);
        }

        public static bool TryFindTemplate(string assetPath, out ImageImportTemplate template)
        {
            template = null;
            if (string.IsNullOrEmpty(assetPath))
            {
                return false;
            }

            var config = ResourceImportConfiguration.Instance;
            if (config.imageTemplates == null)
            {
                return false;
            }

            var normalizedAsset = assetPath.Replace('\\', '/');
            int bestLength = -1;
            for (int i = 0; i < config.imageTemplates.Length; i++)
            {
                var candidate = config.imageTemplates[i];
                if (candidate?.folders == null)
                {
                    continue;
                }

                for (int j = 0; j < candidate.folders.Length; j++)
                {
                    var folder = NormalizeAssetFolder(candidate.folders[j]);
                    if (!IsValidAssetsFolder(folder))
                    {
                        continue;
                    }

                    if (!IsUnderFolder(normalizedAsset, folder))
                    {
                        continue;
                    }

                    if (folder.Length > bestLength)
                    {
                        bestLength = folder.Length;
                        template = candidate;
                    }
                }
            }

            return template != null;
        }

        public static bool IsUnderFolder(string assetPath, string folder)
        {
            var normalizedAsset = assetPath.Replace('\\', '/');
            var normalizedFolder = NormalizeAssetFolder(folder);
            return normalizedAsset == normalizedFolder
                   || normalizedAsset.StartsWith(normalizedFolder + "/", StringComparison.Ordinal);
        }

        public static void Apply(TextureImporter importer, ImageImportTemplate template)
        {
            if (importer == null || template == null)
            {
                return;
            }

            importer.textureType = template.textureType;
            importer.textureShape = template.textureShape;
            importer.sRGBTexture = template.sRGBTexture;
            importer.alphaSource = template.alphaSource;
            importer.alphaIsTransparency = template.alphaIsTransparency;
            importer.npotScale = template.npotScale;
            importer.isReadable = template.isReadable;
            importer.streamingMipmaps = template.streamingMipmaps;

            importer.mipmapEnabled = template.mipmapEnabled;
            importer.borderMipmap = template.borderMipmap;
            importer.mipMapsPreserveCoverage = template.mipMapsPreserveCoverage;
            importer.fadeout = template.fadeout;

            importer.filterMode = template.filterMode;
            importer.anisoLevel = template.anisoLevel;
            importer.wrapMode = template.wrapMode;

            if (template.textureType == TextureImporterType.Sprite)
            {
                importer.spriteImportMode = template.spriteImportMode;
                importer.spritePixelsPerUnit = template.spritePixelsPerUnit;
                var textureSettings = new TextureImporterSettings();
                importer.ReadTextureSettings(textureSettings);
                textureSettings.spriteMeshType = template.spriteMeshType;
                textureSettings.spriteExtrude = template.spriteExtrude;
                importer.SetTextureSettings(textureSettings);
            }

            importer.maxTextureSize = template.maxTextureSize;
            importer.textureCompression = template.textureCompression;
            importer.crunchedCompression = template.crunchedCompression;
            importer.compressionQuality = template.compressionQuality;

            ApplyPlatform(importer, "DefaultTexturePlatform", new ImagePlatformOverride
            {
                overridden = false,
                maxTextureSize = template.maxTextureSize,
                format = TextureImporterFormat.Automatic,
                compressionQuality = template.compressionQuality
            });
            ApplyPlatform(importer, "Android", template.android);
            ApplyPlatform(importer, "iPhone", template.iOS);
            ApplyPlatform(importer, "Standalone", template.standalone);
            ApplyPlatform(importer, "WebGL", template.webGL);
        }

        public static List<string> CollectImagePaths(IEnumerable<string> folders)
        {
            var result = new List<string>();
            if (folders == null)
            {
                return result;
            }

            var validFolders = new List<string>();
            foreach (var folder in folders)
            {
                var normalized = NormalizeAssetFolder(folder);
                if (!IsValidAssetsFolder(normalized) || !AssetDatabase.IsValidFolder(normalized))
                {
                    continue;
                }

                validFolders.Add(normalized);
            }

            if (validFolders.Count == 0)
            {
                return result;
            }

            var guids = AssetDatabase.FindAssets("t:Texture", validFolders.ToArray());
            var unique = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                if (!IsImageAsset(path) || !unique.Add(path))
                {
                    continue;
                }

                result.Add(path);
            }

            result.Sort(StringComparer.OrdinalIgnoreCase);
            return result;
        }

        public static void ReimportFolders(IEnumerable<string> folders, string title, ImageImportTemplate forceTemplate = null)
        {
            var paths = CollectImagePaths(folders);
            if (paths.Count == 0)
            {
                EditorUtility.DisplayDialog(title, "没有找到可导入的图片。请确认文件夹以 Assets 开头且目录存在。", "确定");
                return;
            }

            if (!EditorUtility.DisplayDialog(title, $"将按模板设置重新导入 {paths.Count} 张图片，是否继续？", "导入", "取消"))
            {
                return;
            }

            try
            {
                ResourceImportPostprocessor.ForceTemplate = forceTemplate;
                SpritePostprocessor.SuspendAutoProcess = true;
                AssetDatabase.StartAssetEditing();
                for (int i = 0; i < paths.Count; i++)
                {
                    if (EditorUtility.DisplayCancelableProgressBar(title, paths[i], (float)i / paths.Count))
                    {
                        break;
                    }

                    AssetDatabase.ImportAsset(paths[i], ImportAssetOptions.ForceUpdate);
                }
            }
            finally
            {
                AssetDatabase.StopAssetEditing();
                ResourceImportPostprocessor.ForceTemplate = null;
                SpritePostprocessor.SuspendAutoProcess = false;
                EditorUtility.ClearProgressBar();
                AssetDatabase.Refresh();
            }

            Debug.Log($"{title} 完成，处理图片 {paths.Count} 张。");
        }

        private static void ApplyPlatform(TextureImporter importer, string platform, ImagePlatformOverride settings)
        {
            if (settings == null)
            {
                return;
            }

            var platformSettings = importer.GetPlatformTextureSettings(platform);
            platformSettings.name = platform;
            platformSettings.overridden = settings.overridden;
            platformSettings.maxTextureSize = settings.maxTextureSize;
            platformSettings.format = settings.format;
            platformSettings.compressionQuality = settings.compressionQuality;
            importer.SetPlatformTextureSettings(platformSettings);
        }
    }
}
