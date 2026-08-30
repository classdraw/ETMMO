using UnityEditor;

namespace ET
{
    public class ResourceImportPostprocessor : AssetPostprocessor
    {
        internal static ImageImportTemplate ForceTemplate;

        private void OnPreprocessTexture()
        {
            if (!ResourceImportApplier.IsImageAsset(assetPath))
            {
                return;
            }

            ImageImportTemplate template = ForceTemplate;
            if (template == null)
            {
                var config = ResourceImportConfiguration.Instance;
                if (config == null || !config.enableAutoApply)
                {
                    return;
                }

                if (!ResourceImportApplier.TryFindTemplate(assetPath, out template))
                {
                    return;
                }
            }

            ResourceImportApplier.Apply(assetImporter as TextureImporter, template);
        }
    }
}
