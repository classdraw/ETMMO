using Unity.Mathematics;
using UnityEngine;

namespace ET.Client
{
    [FriendOf(typeof(FollowComponent))]
    public static class CastEmptyBulletViewHelper
    {
        public static async ETTask CreateView(Scene scene, Unit bulletUnit, Unit caster, Unit target, int effectConfigId, int flyTimeMs)
        {
            if (bulletUnit == null || bulletUnit.IsDisposed || caster == null || caster.IsDisposed
                || target == null || target.IsDisposed)
            {
                return;
            }

            EffectConfig effectConfig = EffectConfigCategory.Instance.Get(effectConfigId);
            float3 spawnPosition = UnitFactory.GetEffectSpawnPosition(caster, effectConfig);
            bulletUnit.Position = spawnPosition;

            string assetPath = EffectHelper.GetEffectAssetPath(effectConfigId);
            if (string.IsNullOrEmpty(assetPath))
            {
                bulletUnit.Dispose();
                return;
            }

            PoolComponent poolComponent = scene.GetComponent<PoolComponent>() ?? scene.Root().GetComponent<PoolComponent>();
            if (poolComponent == null)
            {
                Log.Error("CastEmptyBulletViewHelper PoolComponent is null");
                bulletUnit.Dispose();
                return;
            }

            GameObject effectGo = await poolComponent.GetEffect(assetPath);
            if (effectGo == null)
            {
                Log.Error($"CastEmptyBulletViewHelper GetEffect failed, assetPath={assetPath}");
                bulletUnit.Dispose();
                return;
            }

            GlobalComponent globalComponent = scene.Root().GetComponent<GlobalComponent>();
            effectGo.transform.SetParent(globalComponent.Unit, true);
            effectGo.transform.position = bulletUnit.Position;
            effectGo.transform.localScale = ToUnityScaleVector3(effectConfig.Scale);
            effectGo.name = $"EmptyBullet_{bulletUnit.Id}_{effectConfig.Model}";

            ReferenceParticleCollector particleCollector = effectGo.GetComponent<ReferenceParticleCollector>();
            if (particleCollector != null)
            {
                particleCollector.PlayAll();
            }

            GameObjectComponent gameObjectComponent = bulletUnit.AddComponent<GameObjectComponent>();
            gameObjectComponent.GameObject = effectGo;
            bulletUnit.AddComponent<ReUseComponent, string>(assetPath);

            FollowComponent followComponent = bulletUnit.GetComponent<FollowComponent>();
            if (followComponent == null || followComponent.IsDisposed)
            {
                bulletUnit.Dispose();
                return;
            }

            followComponent.IsReady = true;
        }

        private static Vector3 ToUnityScaleVector3(float[] values)
        {
            if (values == null || values.Length != 3)
            {
                return Vector3.one;
            }

            return new Vector3(values[0], values[1], values[2]);
        }
    }
}
