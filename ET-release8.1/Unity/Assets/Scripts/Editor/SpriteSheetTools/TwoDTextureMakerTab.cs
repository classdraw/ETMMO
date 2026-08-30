using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace ET
{
    /// <summary>
    /// 从源贴图 Alpha 轮廓生成 Mask 与切线空间 Normal，供序列帧窗口「2D贴图制作」页签使用。
    /// </summary>
    [Serializable]
    public sealed class TwoDTextureMakerTab
    {
        [SerializeField] private Texture2D sourceTexture;
        [SerializeField, Range(0, 8)] private int outlineOuterPixels = 1;
        [SerializeField, Range(0, 8)] private int outlineInnerPixels;
        [SerializeField] private Color outlineColor = Color.red;
        [SerializeField, Range(0f, 1f)] private float alphaThreshold = 0.01f;
        [SerializeField, Range(0.01f, 8f)] private float normalStrength = 2f;

        public void OnGUI()
        {
            EditorGUILayout.HelpBox(
                "Mask：角色主体为黑色、描边为红色，其余区域透明。\n" +
                "描边像素 = 外扩 + 内缩：外扩画在轮廓外，内缩从轮廓往贴图内部覆盖主体。\n" +
                "Normal：从贴图 Alpha 轮廓计算切线空间 2D 法线。\n" +
                "将图片拖入下方输入框后，两个输出自动生成在该图片同目录。",
                MessageType.Info);

            sourceTexture = (Texture2D)EditorGUILayout.ObjectField("源贴图", sourceTexture, typeof(Texture2D), false);
            if (TryGetOutputPaths(sourceTexture, out string generatedMaskPath, out string generatedNormalPath))
            {
                EditorGUILayout.LabelField("Mask 输出", generatedMaskPath);
                EditorGUILayout.LabelField("Normal 输出", generatedNormalPath);
            }
            else if (sourceTexture != null)
            {
                EditorGUILayout.HelpBox("请拖入 Project 面板内 Assets/ 文件夹中的图片。", MessageType.Warning);
            }

            EditorGUILayout.Space(4f);
            outlineOuterPixels = EditorGUILayout.IntSlider("外扩像素", outlineOuterPixels, 0, 8);
            outlineInnerPixels = EditorGUILayout.IntSlider("内缩像素", outlineInnerPixels, 0, 8);
            EditorGUILayout.LabelField("实际描边像素", (outlineOuterPixels + outlineInnerPixels).ToString());
            outlineColor = EditorGUILayout.ColorField("描边颜色", outlineColor);
            alphaThreshold = EditorGUILayout.Slider("Alpha 阈值", alphaThreshold, 0f, 1f);
            normalStrength = EditorGUILayout.Slider("法线强度", normalStrength, 0.01f, 8f);

            EditorGUILayout.Space(8f);
            using (new EditorGUI.DisabledScope(sourceTexture == null))
            {
                if (GUILayout.Button("生成 Mask 与 Normal", GUILayout.Height(32f)))
                {
                    Generate();
                }
            }
        }

        private static bool TryGetOutputPaths(Texture2D texture, out string generatedMaskPath, out string generatedNormalPath)
        {
            generatedMaskPath = null;
            generatedNormalPath = null;
            if (texture == null)
            {
                return false;
            }

            string sourcePath = AssetDatabase.GetAssetPath(texture);
            if (string.IsNullOrEmpty(sourcePath))
            {
                return false;
            }

            string folder = Path.GetDirectoryName(sourcePath)?.Replace('\\', '/');
            string name = Path.GetFileNameWithoutExtension(sourcePath);
            string rootName = name.EndsWith("_Base_Basic", StringComparison.OrdinalIgnoreCase)
                ? name.Substring(0, name.Length - "_Base_Basic".Length)
                : name;

            generatedMaskPath = $"{folder}/{rootName}_Mask.png";
            generatedNormalPath = $"{folder}/{rootName}_Normal.png";
            return true;
        }

        private void Generate()
        {
            if (!TryGetOutputPaths(sourceTexture, out string maskPath, out string normalPath) ||
                !TryValidatePath(maskPath, "Mask", out string normalizedMaskPath) ||
                !TryValidatePath(normalPath, "Normal", out string normalizedNormalPath))
            {
                return;
            }

            if (normalizedMaskPath.Equals(normalizedNormalPath, StringComparison.OrdinalIgnoreCase))
            {
                EditorUtility.DisplayDialog("输出路径无效", "Mask 与 Normal 必须输出到不同文件。", "确定");
                return;
            }

            if (!ConfirmOverwrite(normalizedMaskPath) || !ConfirmOverwrite(normalizedNormalPath))
            {
                return;
            }

            try
            {
                Color[] sourcePixels = ReadPixels(sourceTexture, out int width, out int height);
                bool[] opaque = CreateOpaqueMask(sourcePixels, alphaThreshold);
                WriteMask(normalizedMaskPath, width, height, opaque);
                WriteNormal(normalizedNormalPath, width, height, sourcePixels, opaque);

                string sourcePath = AssetDatabase.GetAssetPath(sourceTexture);
                ConfigureSpriteImporter(sourcePath, normalizedMaskPath, false);
                ConfigureSpriteImporter(sourcePath, normalizedNormalPath, true);
                AssetDatabase.Refresh();

                Selection.objects = new UnityEngine.Object[]
                {
                    AssetDatabase.LoadAssetAtPath<Texture2D>(normalizedMaskPath),
                    AssetDatabase.LoadAssetAtPath<Texture2D>(normalizedNormalPath)
                };
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                EditorUtility.DisplayDialog("生成失败", exception.Message, "确定");
            }
        }

        private static bool TryValidatePath(string path, string label, out string normalizedPath)
        {
            normalizedPath = (path ?? string.Empty).Replace('\\', '/').Trim();
            if (!normalizedPath.StartsWith("Assets/", StringComparison.Ordinal) ||
                !normalizedPath.EndsWith(".png", StringComparison.OrdinalIgnoreCase))
            {
                EditorUtility.DisplayDialog("输出路径无效", $"{label} 输出必须是 Assets/ 下的 .png 文件。", "确定");
                return false;
            }

            string folder = Path.GetDirectoryName(normalizedPath);
            if (string.IsNullOrEmpty(folder) || !AssetDatabase.IsValidFolder(folder))
            {
                EditorUtility.DisplayDialog("输出路径无效", $"目录不存在：{folder}", "确定");
                return false;
            }

            return true;
        }

        private static bool ConfirmOverwrite(string assetPath)
        {
            if (!File.Exists(assetPath))
            {
                return true;
            }

            return EditorUtility.DisplayDialog(
                "覆盖现有贴图？",
                $"将覆盖 {assetPath}。",
                "覆盖",
                "取消");
        }

        private static Color[] ReadPixels(Texture2D source, out int width, out int height)
        {
            width = source.width;
            height = source.height;
            RenderTexture temporary = RenderTexture.GetTemporary(width, height, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
            RenderTexture previous = RenderTexture.active;
            Texture2D copy = new Texture2D(width, height, TextureFormat.RGBA32, false, true);

            try
            {
                Graphics.Blit(source, temporary);
                RenderTexture.active = temporary;
                copy.ReadPixels(new Rect(0f, 0f, width, height), 0, 0);
                copy.Apply(false, false);
                return copy.GetPixels();
            }
            finally
            {
                RenderTexture.active = previous;
                RenderTexture.ReleaseTemporary(temporary);
                UnityEngine.Object.DestroyImmediate(copy);
            }
        }

        private static bool[] CreateOpaqueMask(Color[] pixels, float threshold)
        {
            bool[] opaque = new bool[pixels.Length];
            for (int i = 0; i < pixels.Length; i++)
            {
                opaque[i] = pixels[i].a > threshold;
            }

            return opaque;
        }

        private void WriteMask(string path, int width, int height, bool[] opaque)
        {
            Color32[] pixels = new Color32[opaque.Length];
            Color32 opaqueBlack = new Color32(0, 0, 0, 255);
            Color32 redOutline = (Color32)outlineColor;
            redOutline.a = 255;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int index = ToIndex(x, y, width);
                    if (opaque[index])
                    {
                        pixels[index] = IsWithinDistanceOf(x, y, width, height, opaque, outlineInnerPixels, false)
                            ? redOutline
                            : opaqueBlack;
                    }
                    else if (IsWithinDistanceOf(x, y, width, height, opaque, outlineOuterPixels, true))
                    {
                        pixels[index] = redOutline;
                    }
                    else
                    {
                        pixels[index] = new Color32(0, 0, 0, 0);
                    }
                }
            }

            WritePng(path, width, height, pixels);
        }

        private void WriteNormal(string path, int width, int height, Color[] sourcePixels, bool[] opaque)
        {
            Color32[] pixels = new Color32[opaque.Length];
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int index = ToIndex(x, y, width);
                    float centerAlpha = sourcePixels[index].a;
                    if (!opaque[index])
                    {
                        pixels[index] = new Color32(128, 128, 255, 0);
                        continue;
                    }

                    float sobelX =
                        SampleAlpha(sourcePixels, x + 1, y - 1, width, height) +
                        2f * SampleAlpha(sourcePixels, x + 1, y, width, height) +
                        SampleAlpha(sourcePixels, x + 1, y + 1, width, height) -
                        SampleAlpha(sourcePixels, x - 1, y - 1, width, height) -
                        2f * SampleAlpha(sourcePixels, x - 1, y, width, height) -
                        SampleAlpha(sourcePixels, x - 1, y + 1, width, height);
                    float sobelY =
                        SampleAlpha(sourcePixels, x - 1, y + 1, width, height) +
                        2f * SampleAlpha(sourcePixels, x, y + 1, width, height) +
                        SampleAlpha(sourcePixels, x + 1, y + 1, width, height) -
                        SampleAlpha(sourcePixels, x - 1, y - 1, width, height) -
                        2f * SampleAlpha(sourcePixels, x, y - 1, width, height) -
                        SampleAlpha(sourcePixels, x + 1, y - 1, width, height);

                    Vector3 normal = new Vector3(-sobelX * normalStrength, -sobelY * normalStrength, 1f).normalized;
                    pixels[index] = new Color32(
                        (byte)Mathf.RoundToInt((normal.x * 0.5f + 0.5f) * 255f),
                        (byte)Mathf.RoundToInt((normal.y * 0.5f + 0.5f) * 255f),
                        (byte)Mathf.RoundToInt((normal.z * 0.5f + 0.5f) * 255f),
                        (byte)Mathf.RoundToInt(centerAlpha * 255f));
                }
            }

            WritePng(path, width, height, pixels);
        }

        private static bool IsWithinDistanceOf(
            int x, int y, int width, int height, bool[] opaque, int radius, bool targetOpaque)
        {
            if (radius <= 0)
            {
                return false;
            }

            int radiusSquared = radius * radius;
            for (int offsetY = -radius; offsetY <= radius; offsetY++)
            {
                for (int offsetX = -radius; offsetX <= radius; offsetX++)
                {
                    if (offsetX * offsetX + offsetY * offsetY > radiusSquared)
                    {
                        continue;
                    }

                    int sampleX = x + offsetX;
                    int sampleY = y + offsetY;
                    if (sampleX < 0 || sampleX >= width || sampleY < 0 || sampleY >= height)
                    {
                        continue;
                    }

                    if (opaque[ToIndex(sampleX, sampleY, width)] == targetOpaque)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private static float SampleAlpha(Color[] pixels, int x, int y, int width, int height)
        {
            x = Mathf.Clamp(x, 0, width - 1);
            y = Mathf.Clamp(y, 0, height - 1);
            return pixels[ToIndex(x, y, width)].a;
        }

        private static int ToIndex(int x, int y, int width) => y * width + x;

        private static void WritePng(string assetPath, int width, int height, Color32[] pixels)
        {
            Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false, true);
            try
            {
                texture.SetPixels32(pixels);
                texture.Apply(false, false);
                File.WriteAllBytes(assetPath, texture.EncodeToPNG());
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(texture);
            }
        }

        private static void ConfigureSpriteImporter(string sourcePath, string outputPath, bool isNormalTexture)
        {
            AssetDatabase.ImportAsset(outputPath, ImportAssetOptions.ForceUpdate);
            TextureImporter sourceImporter = AssetImporter.GetAtPath(sourcePath) as TextureImporter;
            TextureImporter outputImporter = AssetImporter.GetAtPath(outputPath) as TextureImporter;
            if (sourceImporter == null || outputImporter == null)
            {
                throw new InvalidOperationException("无法读取源贴图或输出贴图的 Texture Importer。");
            }

            TextureImporterSettings settings = new TextureImporterSettings();
            sourceImporter.ReadTextureSettings(settings);
            outputImporter.SetTextureSettings(settings);
            outputImporter.textureType = sourceImporter.textureType;
            outputImporter.spriteImportMode = sourceImporter.spriteImportMode;
            outputImporter.spritePixelsPerUnit = sourceImporter.spritePixelsPerUnit;
            outputImporter.spritePivot = sourceImporter.spritePivot;
            outputImporter.spriteBorder = sourceImporter.spriteBorder;
            outputImporter.filterMode = sourceImporter.filterMode;
            outputImporter.wrapMode = sourceImporter.wrapMode;
            outputImporter.mipmapEnabled = sourceImporter.mipmapEnabled;
            outputImporter.alphaIsTransparency = sourceImporter.alphaIsTransparency;
            outputImporter.sRGBTexture = isNormalTexture ? false : sourceImporter.sRGBTexture;

            string outputName = Path.GetFileNameWithoutExtension(outputPath);
            SpriteMetaData[] sprites = sourceImporter.spritesheet;
            for (int i = 0; i < sprites.Length; i++)
            {
                sprites[i].name = $"{outputName}_{i}";
            }

            outputImporter.spritesheet = sprites;
            outputImporter.SaveAndReimport();
        }
    }
}
