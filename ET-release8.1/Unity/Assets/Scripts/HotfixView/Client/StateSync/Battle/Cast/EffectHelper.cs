using UnityEngine;

namespace ET.Client
{
    /// <summary>
    /// 特效管理，外部统一入口，内部委托 PoolComponent。
    /// </summary>
    public static class EffectHelper
    {
        private const string EffectBundlePathPrefix = "Assets/Bundles/Effect/";

        public static string GetEffectAssetPath(int effectConfigId)
        {
            if (!EffectConfigCategory.Instance.Contain(effectConfigId))
            {
                Log.Error($"EffectHelper GetEffectAssetPath failed, EffectConfig not found: {effectConfigId}");
                return null;
            }

            EffectConfig effectConfig = EffectConfigCategory.Instance.Get(effectConfigId);
            if (string.IsNullOrEmpty(effectConfig.Model))
            {
                Log.Error($"EffectHelper GetEffectAssetPath failed, effect model is empty, effectConfigId={effectConfigId}");
                return null;
            }

            return $"{EffectBundlePathPrefix}{effectConfig.Model}.prefab";
        }

        public static async ETTask<GameObject> GetEffect(Scene scene, string key)
        {
            PoolComponent poolComponent = GetPoolComponent(scene);
            if (poolComponent == null)
            {
                Log.Error($"EffectHelper GetEffect failed, PoolComponent is null, key={key}");
                return null;
            }

            return await poolComponent.GetEffect(key);
        }

        public static async ETTask<GameObject> GetEffect(Scene scene, int effectConfigId)
        {
            string assetPath = GetEffectAssetPath(effectConfigId);
            if (string.IsNullOrEmpty(assetPath))
            {
                return null;
            }

            return await GetEffect(scene, assetPath);
        }

        public static void ReturnEffect(Scene scene, string key, GameObject go)
        {
            if (go == null)
            {
                return;
            }

            PoolComponent poolComponent = GetPoolComponent(scene);
            if (poolComponent == null)
            {
                Log.Error($"EffectHelper ReturnEffect failed, PoolComponent is null, key={key}");
                UnityEngine.Object.Destroy(go);
                return;
            }

            poolComponent.ReturnEffect(key, go);
        }

        public static void ReturnEffect(Scene scene, int effectConfigId, GameObject go)
        {
            string assetPath = GetEffectAssetPath(effectConfigId);
            if (string.IsNullOrEmpty(assetPath))
            {
                UnityEngine.Object.Destroy(go);
                return;
            }

            ReturnEffect(scene, assetPath, go);
        }

        private static PoolComponent GetPoolComponent(Scene scene)
        {
            if (scene == null)
            {
                return null;
            }

            PoolComponent poolComponent = scene.GetComponent<PoolComponent>();
            if (poolComponent != null)
            {
                return poolComponent;
            }

            return scene.Root().GetComponent<PoolComponent>();
        }
    }
}
