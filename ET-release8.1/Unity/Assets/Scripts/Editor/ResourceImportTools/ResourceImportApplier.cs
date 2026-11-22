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
            return TryResolve(assetPath, out var result) && (template = result.group) != null;
        }

        public static bool TryResolve(string assetPath, out ImageImportResolveResult result)
        {
            result = null;
            if (string.IsNullOrEmpty(assetPath))
            {
                return false;
            }

            var config = ResourceImportConfiguration.Instance;
            if (config?.imageTemplates == null)
            {
                return false;
            }

            if (ImportPathMatcher.IsWhitelisted(assetPath, config.pathWhitelist))
            {
                return false;
            }

            var normalizedAsset = assetPath.Replace('\\', '/');
            ImageImportTemplate bestGroup = null;
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
                    if (!IsValidAssetsFolder(folder) || !IsUnderFolder(normalizedAsset, folder))
                    {
                        continue;
                    }

                    if (ImportPathMatcher.IsIgnoredByGroup(normalizedAsset, candidate))
                    {
                        continue;
                    }

                    if (folder.Length > bestLength)
                    {
                        bestLength = folder.Length;
                        bestGroup = candidate;
                    }
                }
            }

            if (bestGroup == null)
            {
                return false;
            }

            ImageImportSpecialRule matchedRule = null;
            if (bestGroup.specialRules != null)
            {
                for (int i = 0; i < bestGroup.specialRules.Length; i++)
                {
                    var rule = bestGroup.specialRules[i];
                    if (rule != null && ImportPathMatcher.RuleMatches(normalizedAsset, rule))
                    {
                        matchedRule = rule;
                    }
                }
            }

            result = new ImageImportResolveResult
            {
                group = bestGroup,
                matchedRule = matchedRule
            };
            return true;
        }

        public static bool ShouldManageTexture(ImageImportResolveResult resolve)
        {
            if (resolve?.group == null)
            {
                return false;
            }

            if (resolve.matchedRule?.applyMask != null && resolve.matchedRule.applyMask.HasAny())
            {
                return true;
            }

            resolve.group.applyMask ??= new ImageImportFieldApplyMask();
            return resolve.group.applyMask.HasAny();
        }

        public static bool TryGetEffectiveTextureType(ImageImportResolveResult resolve, out TextureImporterType textureType)
        {
            textureType = default;
            if (resolve?.group == null)
            {
                return false;
            }

            if (resolve.matchedRule?.applyMask != null && resolve.matchedRule.applyMask.textureType)
            {
                textureType = resolve.matchedRule.settings.textureType;
                return true;
            }

            resolve.group.applyMask ??= new ImageImportFieldApplyMask();
            if (resolve.group.applyMask.textureType)
            {
                textureType = resolve.group.textureType;
                return true;
            }

            return false;
        }

        public static ImageImportTemplate CloneTemplate(ImageImportTemplate source)
        {
            if (source == null)
            {
                return null;
            }

            return JsonUtility.FromJson<ImageImportTemplate>(JsonUtility.ToJson(source));
        }

        public static bool IsUnderFolder(string assetPath, string folder)
        {
            var normalizedAsset = assetPath.Replace('\\', '/');
            var normalizedFolder = NormalizeAssetFolder(folder);
            return normalizedAsset == normalizedFolder
                   || normalizedAsset.StartsWith(normalizedFolder + "/", StringComparison.Ordinal);
        }

        public static void Apply(TextureImporter importer, ImageImportResolveResult resolve)
        {
            if (importer == null || resolve?.group == null)
            {
                return;
            }

            var group = resolve.group;
            var rule = resolve.matchedRule;
            var groupMask = group.applyMask ?? new ImageImportFieldApplyMask();
            var ruleMask = rule?.applyMask;
            var ruleSettings = rule?.settings ?? group;

            if (IsOn(groupMask.textureType, ruleMask?.textureType))
            {
                importer.textureType = Pick(ruleMask?.textureType, groupMask.textureType, ruleSettings.textureType, group.textureType);
            }

            if (IsOn(groupMask.textureShape, ruleMask?.textureShape))
            {
                importer.textureShape = Pick(ruleMask?.textureShape, groupMask.textureShape, ruleSettings.textureShape, group.textureShape);
            }

            if (IsOn(groupMask.sRGBTexture, ruleMask?.sRGBTexture))
            {
                importer.sRGBTexture = Pick(ruleMask?.sRGBTexture, groupMask.sRGBTexture, ruleSettings.sRGBTexture, group.sRGBTexture);
            }

            if (IsOn(groupMask.alphaSource, ruleMask?.alphaSource))
            {
                importer.alphaSource = Pick(ruleMask?.alphaSource, groupMask.alphaSource, ruleSettings.alphaSource, group.alphaSource);
            }

            if (IsOn(groupMask.alphaIsTransparency, ruleMask?.alphaIsTransparency))
            {
                importer.alphaIsTransparency = Pick(ruleMask?.alphaIsTransparency, groupMask.alphaIsTransparency, ruleSettings.alphaIsTransparency, group.alphaIsTransparency);
            }

            if (IsOn(groupMask.npotScale, ruleMask?.npotScale))
            {
                importer.npotScale = Pick(ruleMask?.npotScale, groupMask.npotScale, ruleSettings.npotScale, group.npotScale);
            }

            if (IsOn(groupMask.isReadable, ruleMask?.isReadable))
            {
                importer.isReadable = Pick(ruleMask?.isReadable, groupMask.isReadable, ruleSettings.isReadable, group.isReadable);
            }

            if (IsOn(groupMask.streamingMipmaps, ruleMask?.streamingMipmaps))
            {
                importer.streamingMipmaps = Pick(ruleMask?.streamingMipmaps, groupMask.streamingMipmaps, ruleSettings.streamingMipmaps, group.streamingMipmaps);
            }

            if (IsOn(groupMask.mipmapEnabled, ruleMask?.mipmapEnabled))
            {
                importer.mipmapEnabled = Pick(ruleMask?.mipmapEnabled, groupMask.mipmapEnabled, ruleSettings.mipmapEnabled, group.mipmapEnabled);
            }

            if (IsOn(groupMask.borderMipmap, ruleMask?.borderMipmap))
            {
                importer.borderMipmap = Pick(ruleMask?.borderMipmap, groupMask.borderMipmap, ruleSettings.borderMipmap, group.borderMipmap);
            }

            if (IsOn(groupMask.mipMapsPreserveCoverage, ruleMask?.mipMapsPreserveCoverage))
            {
                importer.mipMapsPreserveCoverage = Pick(ruleMask?.mipMapsPreserveCoverage, groupMask.mipMapsPreserveCoverage, ruleSettings.mipMapsPreserveCoverage, group.mipMapsPreserveCoverage);
            }

            if (IsOn(groupMask.fadeout, ruleMask?.fadeout))
            {
                importer.fadeout = Pick(ruleMask?.fadeout, groupMask.fadeout, ruleSettings.fadeout, group.fadeout);
            }

            if (IsOn(groupMask.filterMode, ruleMask?.filterMode))
            {
                importer.filterMode = Pick(ruleMask?.filterMode, groupMask.filterMode, ruleSettings.filterMode, group.filterMode);
            }

            if (IsOn(groupMask.anisoLevel, ruleMask?.anisoLevel))
            {
                importer.anisoLevel = Pick(ruleMask?.anisoLevel, groupMask.anisoLevel, ruleSettings.anisoLevel, group.anisoLevel);
            }

            if (IsOn(groupMask.wrapMode, ruleMask?.wrapMode))
            {
                importer.wrapMode = Pick(ruleMask?.wrapMode, groupMask.wrapMode, ruleSettings.wrapMode, group.wrapMode);
            }

            bool spriteTouched = IsOn(groupMask.spriteImportMode, ruleMask?.spriteImportMode)
                                 || IsOn(groupMask.spritePixelsPerUnit, ruleMask?.spritePixelsPerUnit)
                                 || IsOn(groupMask.spriteMeshType, ruleMask?.spriteMeshType)
                                 || IsOn(groupMask.spriteExtrude, ruleMask?.spriteExtrude);
            if (spriteTouched)
            {
                if (IsOn(groupMask.spriteImportMode, ruleMask?.spriteImportMode))
                {
                    importer.spriteImportMode = Pick(ruleMask?.spriteImportMode, groupMask.spriteImportMode, ruleSettings.spriteImportMode, group.spriteImportMode);
                }

                if (IsOn(groupMask.spritePixelsPerUnit, ruleMask?.spritePixelsPerUnit))
                {
                    importer.spritePixelsPerUnit = Pick(ruleMask?.spritePixelsPerUnit, groupMask.spritePixelsPerUnit, ruleSettings.spritePixelsPerUnit, group.spritePixelsPerUnit);
                }

                var textureSettings = new TextureImporterSettings();
                importer.ReadTextureSettings(textureSettings);
                bool spriteSettingsDirty;
                spriteSettingsDirty = false;
                if (IsOn(groupMask.spriteMeshType, ruleMask?.spriteMeshType))
                {
                    textureSettings.spriteMeshType = Pick(ruleMask?.spriteMeshType, groupMask.spriteMeshType, ruleSettings.spriteMeshType, group.spriteMeshType);
                    spriteSettingsDirty = true;
                }

                if (IsOn(groupMask.spriteExtrude, ruleMask?.spriteExtrude))
                {
                    textureSettings.spriteExtrude = Pick(ruleMask?.spriteExtrude, groupMask.spriteExtrude, ruleSettings.spriteExtrude, group.spriteExtrude);
                    spriteSettingsDirty = true;
                }

                if (spriteSettingsDirty)
                {
                    importer.SetTextureSettings(textureSettings);
                }
            }

            if (IsOn(groupMask.maxTextureSize, ruleMask?.maxTextureSize))
            {
                importer.maxTextureSize = Pick(ruleMask?.maxTextureSize, groupMask.maxTextureSize, ruleSettings.maxTextureSize, group.maxTextureSize);
            }

            if (IsOn(groupMask.textureCompression, ruleMask?.textureCompression))
            {
                importer.textureCompression = Pick(ruleMask?.textureCompression, groupMask.textureCompression, ruleSettings.textureCompression, group.textureCompression);
            }

            if (IsOn(groupMask.crunchedCompression, ruleMask?.crunchedCompression))
            {
                importer.crunchedCompression = Pick(ruleMask?.crunchedCompression, groupMask.crunchedCompression, ruleSettings.crunchedCompression, group.crunchedCompression);
            }

            if (IsOn(groupMask.compressionQuality, ruleMask?.compressionQuality))
            {
                importer.compressionQuality = Pick(ruleMask?.compressionQuality, groupMask.compressionQuality, ruleSettings.compressionQuality, group.compressionQuality);
            }

            if (IsOn(groupMask.android, ruleMask?.android))
            {
                ApplyPlatform(importer, "Android", PickPlatform(ruleMask?.android, groupMask.android, ruleSettings.android, group.android));
            }

            if (IsOn(groupMask.iOS, ruleMask?.iOS))
            {
                ApplyPlatform(importer, "iPhone", PickPlatform(ruleMask?.iOS, groupMask.iOS, ruleSettings.iOS, group.iOS));
            }

            if (IsOn(groupMask.standalone, ruleMask?.standalone))
            {
                ApplyPlatform(importer, "Standalone", PickPlatform(ruleMask?.standalone, groupMask.standalone, ruleSettings.standalone, group.standalone));
            }

            if (IsOn(groupMask.webGL, ruleMask?.webGL))
            {
                ApplyPlatform(importer, "WebGL", PickPlatform(ruleMask?.webGL, groupMask.webGL, ruleSettings.webGL, group.webGL));
            }
        }

        public static List<string> CollectImagePaths(IEnumerable<string> folders, IEnumerable<string> ignoreFolders = null)
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

                if (ignoreFolders != null)
                {
                    bool ignored = false;
                    foreach (var ignore in ignoreFolders)
                    {
                        if (IsUnderFolder(path, ignore))
                        {
                            ignored = true;
                            break;
                        }
                    }

                    if (ignored)
                    {
                        continue;
                    }
                }

                var config = ResourceImportConfiguration.Instance;
                if (config != null && ImportPathMatcher.IsWhitelisted(path, config.pathWhitelist))
                {
                    continue;
                }

                result.Add(path);
            }

            result.Sort(StringComparer.OrdinalIgnoreCase);
            return result;
        }

        public static void ReimportFolders(IEnumerable<string> folders, string title, ImageImportTemplate limitToGroup = null)
        {
            string[] ignore = limitToGroup?.ignoreFolders;
            var paths = CollectImagePaths(folders, ignore);
            if (paths.Count == 0)
            {
                EditorUtility.DisplayDialog(title, "没有找到可导入的图片。请确认文件夹以 Assets 开头且目录存在。", "确定");
                return;
            }

            if (!EditorUtility.DisplayDialog(title, $"将按组与词缀规则重新导入 {paths.Count} 张图片，是否继续？", "导入", "取消"))
            {
                return;
            }

            try
            {
                ResourceImportPostprocessor.LimitToGroup = limitToGroup;
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
                ResourceImportPostprocessor.LimitToGroup = null;
                SpritePostprocessor.SuspendAutoProcess = false;
                EditorUtility.ClearProgressBar();
                AssetDatabase.Refresh();
            }

            Debug.Log($"{title} 完成，处理图片 {paths.Count} 张。");
        }

        public static void ReimportGroup(ImageImportTemplate group, string title)
        {
            if (group?.folders == null || group.folders.Length == 0)
            {
                EditorUtility.DisplayDialog(title, "当前组未配置组策略目录。", "确定");
                return;
            }

            ReimportFolders(group.folders, title, group);
        }

        private static bool IsOn(bool groupOn, bool? ruleOn)
        {
            if (ruleOn == true)
            {
                return true;
            }

            return groupOn;
        }

        private static T Pick<T>(bool? ruleApply, bool groupApply, T ruleValue, T groupValue)
        {
            if (ruleApply == true)
            {
                return ruleValue;
            }

            return groupValue;
        }

        private static ImagePlatformOverride PickPlatform(bool? ruleMask, bool groupMask, ImagePlatformOverride ruleValue, ImagePlatformOverride groupValue)
        {
            if (ruleMask == true)
            {
                return ruleValue ?? new ImagePlatformOverride();
            }

            return groupValue ?? new ImagePlatformOverride();
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