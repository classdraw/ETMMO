using System;

namespace ET
{
    [Serializable]
    public class ImageImportFieldApplyMask
    {
        public bool textureType;
        public bool textureShape;
        public bool sRGBTexture;
        public bool alphaSource;
        public bool alphaIsTransparency;
        public bool npotScale;
        public bool isReadable;
        public bool streamingMipmaps;

        public bool mipmapEnabled;
        public bool borderMipmap;
        public bool mipMapsPreserveCoverage;
        public bool fadeout;

        public bool filterMode;
        public bool anisoLevel;
        public bool wrapMode;

        public bool spriteImportMode;
        public bool spritePixelsPerUnit;
        public bool spriteMeshType;
        public bool spriteExtrude;

        public bool maxTextureSize;
        public bool textureCompression;
        public bool crunchedCompression;
        public bool compressionQuality;

        public bool android;
        public bool iOS;
        public bool standalone;
        public bool webGL;

        public bool HasAny()
        {
            return textureType || textureShape || sRGBTexture || alphaSource || alphaIsTransparency
                   || npotScale || isReadable || streamingMipmaps || mipmapEnabled || borderMipmap
                   || mipMapsPreserveCoverage || fadeout || filterMode || anisoLevel || wrapMode
                   || spriteImportMode || spritePixelsPerUnit || spriteMeshType || spriteExtrude
                   || maxTextureSize || textureCompression || crunchedCompression || compressionQuality
                   || android || iOS || standalone || webGL;
        }

        public void SetAll(bool on)
        {
            textureType = textureShape = sRGBTexture = alphaSource = alphaIsTransparency = on;
            npotScale = isReadable = streamingMipmaps = on;
            mipmapEnabled = borderMipmap = mipMapsPreserveCoverage = fadeout = on;
            filterMode = anisoLevel = wrapMode = on;
            spriteImportMode = spritePixelsPerUnit = spriteMeshType = spriteExtrude = on;
            maxTextureSize = textureCompression = crunchedCompression = compressionQuality = on;
            android = iOS = standalone = webGL = on;
        }
    }
}