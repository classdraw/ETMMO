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
            if (!ResourceImportApplier.TryFindTemplate(assetPath, out var template) || template == null)
            {
                return false;
            }

            return template.textureType != TextureImporterType.Sprite;
        }
    }
}
