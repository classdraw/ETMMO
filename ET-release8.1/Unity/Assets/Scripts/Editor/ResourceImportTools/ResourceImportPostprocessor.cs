using UnityEditor;

namespace ET
{
    public class ResourceImportPostprocessor : AssetPostprocessor
    {
        internal static ImageImportTemplate LimitToGroup;

        private void OnPreprocessTexture()
        {
            if (!ResourceImportApplier.IsImageAsset(assetPath))
            {
                return;
            }

            var config = ResourceImportConfiguration.Instance;
            bool manualReimport = LimitToGroup != null;
            if (!manualReimport && (config == null || !config.enableAutoApply))
            {
                return;
            }

            if (!ResourceImportApplier.TryResolve(assetPath, out var result) || result?.group == null)
            {
                return;
            }

            if (LimitToGroup != null && result.group != LimitToGroup)
            {
                return;
            }

            if (!ResourceImportApplier.ShouldManageTexture(result))
            {
                return;
            }

            ResourceImportApplier.Apply(assetImporter as TextureImporter, result);
        }
    }
}