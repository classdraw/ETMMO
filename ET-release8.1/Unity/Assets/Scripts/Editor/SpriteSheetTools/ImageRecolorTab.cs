using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace ET
{
    /// <summary>
    /// 将 PNG 中所有 Alpha 非 0 的像素改色，供序列帧窗口「图片改色」页签使用。
    /// </summary>
    [Serializable]
    public sealed class ImageRecolorTab
    {
        [SerializeField] private Texture2D sourceTexture;
        [SerializeField] private Color targetColor = Color.white;
        [SerializeField] private Texture2D previewTexture;

        public void OnGUI()
        {
            EditorGUILayout.HelpBox(
                "拖入一张 PNG 图片，选择目标颜色后点击「替换原图」。\n" +
                "所有 Alpha 不为 0 的像素 RGB 会改为目标颜色，Alpha 保持不变，直接覆盖原 PNG 文件。",
                MessageType.Info);

            EditorGUI.BeginChangeCheck();
            sourceTexture = (Texture2D)EditorGUILayout.ObjectField("源图片", sourceTexture, typeof(Texture2D), false);
            targetColor = EditorGUILayout.ColorField(new GUIContent("目标颜色"), targetColor, true, true, false);
            if (EditorGUI.EndChangeCheck())
            {
                RebuildPreview();
            }

            string sourcePath = GetSourcePath();
            if (sourceTexture != null && string.IsNullOrEmpty(sourcePath))
            {
                EditorGUILayout.HelpBox("请拖入 Project 面板内 Assets/ 文件夹中的 PNG 图片。", MessageType.Warning);
            }
            else if (!string.IsNullOrEmpty(sourcePath))
            {
                EditorGUILayout.LabelField("源文件", sourcePath, EditorStyles.wordWrappedLabel);
                EditorGUILayout.LabelField("尺寸", $"{sourceTexture.width} x {sourceTexture.height}");
            }

            EditorGUILayout.Space(8f);
            using (new EditorGUI.DisabledScope(sourceTexture == null || string.IsNullOrEmpty(sourcePath)))
            {
                if (GUILayout.Button("替换原图", GUILayout.Height(32f)))
                {
                    SaveRecoloredImage();
                }
            }

            Texture2D display = previewTexture != null ? previewTexture : sourceTexture;
            if (display != null)
            {
                EditorGUILayout.Space(8f);
                EditorGUILayout.LabelField(previewTexture != null ? "改色预览" : "原图预览");
                float maxPreview = 320f;
                float scale = Mathf.Min(maxPreview / display.width, maxPreview / display.height, 1f);
                Rect previewRect = GUILayoutUtility.GetRect(
                    display.width * scale,
                    display.height * scale,
                    GUILayout.ExpandWidth(false));
                EditorGUI.DrawTextureTransparent(previewRect, display);
            }
        }

        public void ClearPreview()
        {
            if (previewTexture != null)
            {
                UnityEngine.Object.DestroyImmediate(previewTexture);
                previewTexture = null;
            }
        }

        private string GetSourcePath()
        {
            if (sourceTexture == null)
            {
                return null;
            }

            string path = AssetDatabase.GetAssetPath(sourceTexture);
            if (string.IsNullOrEmpty(path))
            {
                return null;
            }

            return path.Replace('\\', '/');
        }

        private void RebuildPreview()
        {
            ClearPreview();

            if (sourceTexture == null)
            {
                return;
            }

            try
            {
                Color[] pixels = ReadPixels(sourceTexture, out int width, out int height);
                ApplyRecolor(pixels);
                previewTexture = CreatePreviewTexture(width, height, pixels);
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
            }
        }

        private void SaveRecoloredImage()
        {
            string sourcePath = GetSourcePath();
            if (string.IsNullOrEmpty(sourcePath))
            {
                EditorUtility.DisplayDialog("图片改色", "请先拖入有效的 PNG 图片。", "确定");
                return;
            }

            if (!sourcePath.EndsWith(".png", StringComparison.OrdinalIgnoreCase))
            {
                EditorUtility.DisplayDialog("图片改色", "当前仅支持 PNG 图片。", "确定");
                return;
            }

            try
            {
                Color[] pixels = ReadPixels(sourceTexture, out int width, out int height);
                ApplyRecolor(pixels);

                string outputPath = ToAbsolutePath(sourcePath);
                WritePng(outputPath, width, height, pixels);

                if (outputPath.Replace('\\', '/').Contains("/Assets"))
                {
                    AssetDatabase.Refresh();
                }

                sourceTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(sourcePath);
                RebuildPreview();
                EditorUtility.DisplayDialog("图片改色", $"已替换原图：\n{outputPath}", "确定");
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                EditorUtility.DisplayDialog("图片改色", exception.Message, "确定");
            }
        }

        private void ApplyRecolor(Color[] pixels)
        {
            Color rgb = targetColor;
            for (int i = 0; i < pixels.Length; i++)
            {
                if (pixels[i].a <= 0f)
                {
                    continue;
                }

                pixels[i].r = rgb.r;
                pixels[i].g = rgb.g;
                pixels[i].b = rgb.b;
            }
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

        private static Texture2D CreatePreviewTexture(int width, int height, Color[] pixels)
        {
            Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false, true);
            texture.SetPixels(pixels);
            texture.Apply(false, false);
            return texture;
        }

        private static void WritePng(string filePath, int width, int height, Color[] pixels)
        {
            Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false, true);
            try
            {
                texture.SetPixels(pixels);
                texture.Apply(false, false);
                File.WriteAllBytes(filePath, texture.EncodeToPNG());
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(texture);
            }
        }

        private static string ToAbsolutePath(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return path;
            }

            if (Path.IsPathRooted(path))
            {
                return Path.GetFullPath(path);
            }

            return Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), path));
        }
    }
}
