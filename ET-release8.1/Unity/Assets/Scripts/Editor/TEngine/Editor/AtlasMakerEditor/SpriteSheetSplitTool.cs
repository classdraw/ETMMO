namespace TEngine.Editor
{
#if UNITY_EDITOR
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using UnityEditor;
    using UnityEngine;

    /// <summary>
    /// 将 Sprite 模式为 Multiple 的单张纹理，按 Sprite Editor 中的切片导出为同目录下多张 PNG。
    /// </summary>
    public static class SpriteSheetSplitTool
    {
        private const string MenuPath = "Tools/图集工具/拆分图集为单图PNG";

        [MenuItem(MenuPath, false, 25)]
        private static void SplitSelected()
        {
            var texturePaths = CollectTargetTexturePaths();
            if (texturePaths.Count == 0)
            {
                EditorUtility.DisplayDialog("拆分图集", "请在 Project 中选中一张「Texture Type = Sprite」且「Sprite Mode = Multiple」的纹理。", "确定");
                return;
            }

            if (!EditorUtility.DisplayDialog("拆分图集",
                    $"将把 {texturePaths.Count} 张图集按切片导出为 PNG，文件生成在各自纹理同目录下。\n是否继续？",
                    "确定", "取消"))
            {
                return;
            }

            try
            {
                int totalExported = 0;
                for (var i = 0; i < texturePaths.Count; i++)
                {
                    var path = texturePaths[i];
                    EditorUtility.DisplayProgressBar("拆分图集", path, (float)i / texturePaths.Count);
                    totalExported += SplitTextureAtPath(path);
                }

                EditorUtility.DisplayDialog("拆分图集", $"完成，共导出 {totalExported} 张 PNG。", "确定");
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }
        }

        [MenuItem(MenuPath, true)]
        private static bool SplitSelectedValidate()
        {
            return CollectTargetTexturePaths().Count > 0;
        }

        private static List<string> CollectTargetTexturePaths()
        {
            var set = new HashSet<string>();
            foreach (var obj in Selection.objects)
            {
                var path = AssetDatabase.GetAssetPath(obj);
                if (string.IsNullOrEmpty(path)) continue;
                if (!IsSplittableTexturePath(path)) continue;
                set.Add(path.Replace("\\", "/"));
            }

            return set.ToList();
        }

        private static bool IsSplittableTexturePath(string assetPath)
        {
            var importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
            if (importer == null) return false;
            if (importer.textureType != TextureImporterType.Sprite) return false;
            return importer.spriteImportMode == SpriteImportMode.Multiple;
        }

        private static int SplitTextureAtPath(string assetPath)
        {
            var sprites = AssetDatabase.LoadAllAssetsAtPath(assetPath).OfType<Sprite>().Where(s => s != null).ToList();
            if (sprites.Count == 0)
            {
                Debug.LogWarning($"[拆分图集] 未找到子 Sprite：{assetPath}");
                return 0;
            }

            var importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
            if (importer == null) return 0;

            var wasReadable = importer.isReadable;
            if (!wasReadable)
            {
                importer.isReadable = true;
                importer.SaveAndReimport();
            }

            var exported = 0;
            var exportedPairs = new List<(string path, Sprite sprite)>();
            try
            {
                var dir = Path.GetDirectoryName(assetPath);
                if (string.IsNullOrEmpty(dir))
                {
                    return 0;
                }

                var nameCount = new Dictionary<string, int>(StringComparer.Ordinal);

                foreach (var sprite in sprites)
                {
                    var stem = SanitizeFileName(string.IsNullOrEmpty(sprite.name) ? $"Sprite_{exported}" : sprite.name);
                    if (!nameCount.TryGetValue(stem, out var dupIndex))
                    {
                        dupIndex = 0;
                    }

                    nameCount[stem] = dupIndex + 1;
                    var fileName = dupIndex == 0 ? stem : $"{stem}_{dupIndex}";

                    var outPath = Path.Combine(dir, fileName + ".png").Replace("\\", "/");
                    if (WriteSpriteToPng(sprite, outPath))
                    {
                        exported++;
                        exportedPairs.Add((outPath, sprite));
                        Debug.Log($"[拆分图集] {outPath}");
                    }
                }
            }
            finally
            {
                if (!wasReadable && importer != null)
                {
                    importer.isReadable = false;
                    importer.SaveAndReimport();
                }
            }

            if (exportedPairs.Count > 0)
            {
                AssetDatabase.Refresh();
                AssetDatabase.StartAssetEditing();
                try
                {
                    foreach (var pair in exportedPairs)
                    {
                        ApplyImporterToMatchSource(assetPath, pair.path, pair.sprite);
                    }
                }
                finally
                {
                    AssetDatabase.StopAssetEditing();
                }
            }

            return exported;
        }

        private static bool WriteSpriteToPng(Sprite sprite, string assetOutputPath)
        {
            var src = sprite.texture;
            if (src == null)
            {
                Debug.LogWarning($"[拆分图集] Sprite 无纹理：{sprite.name}");
                return false;
            }

            var tr = sprite.textureRect;
            var x = Mathf.RoundToInt(tr.x);
            var y = Mathf.RoundToInt(tr.y);
            var w = Mathf.RoundToInt(tr.width);
            var h = Mathf.RoundToInt(tr.height);
            if (w <= 0 || h <= 0)
            {
                Debug.LogWarning($"[拆分图集] 无效区域 {sprite.name} ({w}x{h})");
                return false;
            }

            Color[] pixels;
            try
            {
                pixels = src.GetPixels(x, y, w, h);
            }
            catch (Exception e)
            {
                Debug.LogError($"[拆分图集] 读取像素失败（请确认纹理已勾选 Read/Write）：{sprite.name}\n{e.Message}");
                return false;
            }

            var newTex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            newTex.SetPixels(pixels);
            newTex.Apply();

            var bytes = newTex.EncodeToPNG();
            UnityEngine.Object.DestroyImmediate(newTex);

            var fullPath = Path.GetFullPath(Path.Combine(Application.dataPath, "..", assetOutputPath));
            File.WriteAllBytes(fullPath, bytes);
            return true;
        }

        /// <summary>
        /// 将目标 PNG 的 TextureImporter 与源图集对齐（含各平台覆盖），并按切片写入 Single 下的 PPU / Border / Pivot。
        /// </summary>
        private static void ApplyImporterToMatchSource(string sourceAssetPath, string destAssetPath, Sprite slice)
        {
            var src = AssetImporter.GetAtPath(sourceAssetPath) as TextureImporter;
            var dst = AssetImporter.GetAtPath(destAssetPath) as TextureImporter;
            if (src == null || dst == null)
            {
                return;
            }

            var settings = new TextureImporterSettings();
            src.ReadTextureSettings(settings);
            settings.textureType = TextureImporterType.Sprite;
            settings.spriteMode = (int)SpriteImportMode.Single;

            settings.spritePixelsPerUnit = slice.pixelsPerUnit;
            settings.spriteBorder = slice.border;
            var tr = slice.textureRect;
            if (tr.width > 0.001f && tr.height > 0.001f)
            {
                settings.spriteAlignment = (int)SpriteAlignment.Custom;
                settings.spritePivot = new Vector2(slice.pivot.x / tr.width, slice.pivot.y / tr.height);
            }

            dst.SetTextureSettings(settings);

            dst.alphaIsTransparency = src.alphaIsTransparency;
            dst.alphaSource = src.alphaSource;
            dst.ignorePngGamma = src.ignorePngGamma;
            dst.isReadable = src.isReadable;
            dst.mipmapEnabled = src.mipmapEnabled;
            dst.npotScale = src.npotScale;
            dst.wrapMode = src.wrapMode;
            dst.anisoLevel = src.anisoLevel;

            dst.SetPlatformTextureSettings(src.GetDefaultPlatformTextureSettings());
           // foreach (var platformName in src.GetPlatformTextureSettingsNames())
           // {
           //     dst.SetPlatformTextureSettings(src.GetPlatformTextureSettings(platformName));
           // }

            dst.SaveAndReimport();
        }

        private static string SanitizeFileName(string name)
        {
            foreach (var c in Path.GetInvalidFileNameChars())
            {
                name = name.Replace(c, '_');
            }

            return string.IsNullOrWhiteSpace(name) ? "Sprite" : name.Trim();
        }
    }
#endif
}
