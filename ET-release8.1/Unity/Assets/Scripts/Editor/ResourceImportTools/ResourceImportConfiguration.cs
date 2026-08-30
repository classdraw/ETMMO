using System;
using TEngine.Editor;
using UnityEditor;
using UnityEngine;

namespace ET
{
    [Serializable]
    public class ImagePlatformOverride
    {
        public bool overridden;
        public int maxTextureSize = 2048;
        public TextureImporterFormat format = TextureImporterFormat.Automatic;
        public int compressionQuality = 50;
    }

    [Serializable]
    public class ImageImportTemplate
    {
        public string name = "新模板";
        public string[] folders = Array.Empty<string>();

        public TextureImporterType textureType = TextureImporterType.Default;
        public TextureImporterShape textureShape = TextureImporterShape.Texture2D;
        public bool sRGBTexture = true;
        public TextureImporterAlphaSource alphaSource = TextureImporterAlphaSource.FromInput;
        public bool alphaIsTransparency = true;
        public TextureImporterNPOTScale npotScale = TextureImporterNPOTScale.ToNearest;
        public bool isReadable;
        public bool streamingMipmaps;

        public bool mipmapEnabled;
        public bool borderMipmap;
        public bool mipMapsPreserveCoverage;
        public bool fadeout;

        public FilterMode filterMode = FilterMode.Bilinear;
        public int anisoLevel = 1;
        public TextureWrapMode wrapMode = TextureWrapMode.Repeat;

        public SpriteImportMode spriteImportMode = SpriteImportMode.Single;
        public float spritePixelsPerUnit = 100f;
        public SpriteMeshType spriteMeshType = SpriteMeshType.FullRect;
        public uint spriteExtrude = 1;

        public int maxTextureSize = 2048;
        public TextureImporterCompression textureCompression = TextureImporterCompression.Compressed;
        public bool crunchedCompression;
        public int compressionQuality = 50;

        public ImagePlatformOverride android = new ImagePlatformOverride
        {
            overridden = true,
            format = TextureImporterFormat.ASTC_6x6
        };

        public ImagePlatformOverride iOS = new ImagePlatformOverride
        {
            format = TextureImporterFormat.ASTC_6x6
        };

        public ImagePlatformOverride standalone = new ImagePlatformOverride();
        public ImagePlatformOverride webGL = new ImagePlatformOverride
        {
            format = TextureImporterFormat.ASTC_6x6
        };
    }

    [TEngine.Editor.FilePath("ProjectSettings/ResourceImportConfiguration.asset")]
    public class ResourceImportConfiguration : EditorScriptableSingleton<ResourceImportConfiguration>
    {
        public bool enableAutoApply = true;
        public ImageImportTemplate[] imageTemplates = { CreateDefaultTemplate() };

        public static ImageImportTemplate CreateDefaultTemplate()
        {
            return new ImageImportTemplate
            {
                name = "默认图片",
                folders = Array.Empty<string>()
            };
        }
    }

    [InitializeOnLoad]
    internal static class ResourceImportConfigurationPersistence
    {
        static ResourceImportConfigurationPersistence()
        {
            EditorApplication.delayCall += () => ResourceImportConfiguration.LoadOrCreate();
            EditorApplication.quitting += OnEditorQuitting;
        }

        private static void OnEditorQuitting()
        {
            ResourceImportConfiguration.Save(true);
        }
    }
}
