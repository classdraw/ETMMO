using TEngine.Editor;
using UnityEditor;
using UnityEngine;

namespace ET
{
    [InitializeOnLoad]
    internal static class ResourceImportAtlasHook
    {
        static ResourceImportAtlasHook()
        {
            SpritePostprocessor.ExtraSkipPredicate = ShouldSkipAtlasProcess;
        }

        private static bool ShouldSkipAtlasProcess(string assetPath)
        {
            if (!ResourceImportApplier.TryResolve(assetPath, out var result))
            {
                return false;
            }

            if (!ResourceImportApplier.TryGetEffectiveTextureType(result, out var textureType))
            {
                return false;
            }

            return textureType != TextureImporterType.Sprite;
        }
    }
}