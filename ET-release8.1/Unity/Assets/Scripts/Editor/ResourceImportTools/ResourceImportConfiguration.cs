using System;
using TEngine.Editor;
using UnityEditor;
using UnityEngine;

namespace ET
{
    public enum ImportPathMatchKind
    {
        FilenameStartsWith,
        FilenameContains,
        FilenameEndsWith,
        DirectoryContains,
        PathRegex,
        FilenameEqual,
        DirectoryStartsWith,
        DirectoryEndsWith,
        DirectoryEqual
    }

    [Serializable]
    public class ImagePlatformOverride
    {
        public bool overridden;
        public int maxTextureSize = 2048;
        public TextureImporterFormat format = TextureImporterFormat.Automatic;
        public int compressionQuality = 50;
    }

    [Serializable]
    public class ImageImportSpecialRule
    {
        public ImportPathMatchKind matchKind = ImportPathMatchKind.DirectoryContains;
        public string pattern = string.Empty;
        public ImageImportFieldApplyMask applyMask = new ImageImportFieldApplyMask();
        public ImageImportTemplate settings = new ImageImportTemplate();
    }

    [Serializable]
    public class ImageImportTemplate
    {
        public int applyMaskVersion = 1;
        public ImageImportFieldApplyMask applyMask = new ImageImportFieldApplyMask();

        public string name = "新组";
        public string[] folders = Array.Empty<string>();
        public string[] ignoreFolders = Array.Empty<string>();
        public ImageImportSpecialRule[] specialRules = Array.Empty<ImageImportSpecialRule>();

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

    [Serializable]
    public class ImageImportResolveResult
    {
        public ImageImportTemplate group;
        public ImageImportSpecialRule matchedRule;
    }

    [TEngine.Editor.FilePath("ProjectSettings/ResourceImportConfiguration.asset")]
    public class ResourceImportConfiguration : EditorScriptableSingleton<ResourceImportConfiguration>
    {
        public bool enableAutoApply = true;
        public bool saveOnClose = true;
        public string pathWhitelist = "DontProcess,Standard Assets,Plugins";

        public ImageImportTemplate[] imageTemplates = { CreateDefaultTemplate() };

        public static ImageImportTemplate CreateDefaultTemplate()
        {
            return new ImageImportTemplate
            {
                applyMaskVersion = 1,
                applyMask = new ImageImportFieldApplyMask(),
                name = "默认图片",
                folders = Array.Empty<string>(),
                ignoreFolders = Array.Empty<string>(),
                specialRules = Array.Empty<ImageImportSpecialRule>()
            };
        }

        public static ImageImportSpecialRule CreateDefaultSpecialRule()
        {
            return new ImageImportSpecialRule
            {
                matchKind = ImportPathMatchKind.DirectoryContains,
                pattern = string.Empty,
                settings = CreateRuleSettingsStub()
            };
        }

        public static ImageImportTemplate CreateRuleSettingsStub()
        {
            var t = CreateDefaultTemplate();
            t.folders = Array.Empty<string>();
            t.ignoreFolders = Array.Empty<string>();
            t.specialRules = Array.Empty<ImageImportSpecialRule>();
            t.applyMask = new ImageImportFieldApplyMask();
            t.applyMaskVersion = 1;
            return t;
        }

        public static void MigrateApplyMask(ImageImportTemplate template)
        {
            if (template == null)
            {
                return;
            }

            template.applyMask ??= new ImageImportFieldApplyMask();
            if (template.applyMaskVersion != 0)
            {
                return;
            }

            template.applyMask.SetAll(true);
            template.applyMaskVersion = 1;
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
            var config = ResourceImportConfiguration.Instance;
            if (config != null && config.saveOnClose)
            {
                ResourceImportConfiguration.Save(true);
            }
        }
    }
}