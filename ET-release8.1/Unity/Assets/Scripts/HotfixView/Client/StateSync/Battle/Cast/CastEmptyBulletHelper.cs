using Unity.Mathematics;
using UnityEngine;

namespace ET.Client
{
    public static class CastEmptyBulletHelper
    {
        private const int EmptyBulletUnitConfigId = 9001;

        public static async ETTask Create(Scene scene, Unit caster, Unit target, int actionId)
        {
            if (caster == null || caster.IsDisposed || target == null || target.IsDisposed)
            {
                return;
            }

            if (!ActionsConfigCategory.Instance.Contain(actionId))
            {
                Log.Error($"CastEmptyBulletHelper ActionsConfig not found: {actionId}");
                return;
            }

            ActionsConfig actionsConfig = ActionsConfigCategory.Instance.Get(actionId);
            if (actionsConfig.ActionsParam == null || actionsConfig.ActionsParam.Length < 2)
            {
                Log.Error($"CastEmptyBulletHelper ActionsParam invalid, actionId={actionId}");
                return;
            }

            int effectConfigId = actionsConfig.ActionsParam[0];
            int flyTimeMs = actionsConfig.ActionsParam[1];
            if (flyTimeMs <= 0)
            {
                Log.Error($"CastEmptyBulletHelper flyTimeMs invalid, actionId={actionId}, flyTimeMs={flyTimeMs}");
                return;
            }

            if (!EffectConfigCategory.Instance.Contain(effectConfigId))
            {
                Log.Error($"CastEmptyBulletHelper EffectConfig not found: {effectConfigId}");
                return;
            }

            string assetPath = EffectHelper.GetEffectAssetPath(effectConfigId);
            if (string.IsNullOrEmpty(assetPath))
            {
                return;
            }

            PoolComponent poolComponent = scene.GetComponent<PoolComponent>() ?? scene.Root().GetComponent<PoolComponent>();
            if (poolComponent == null)
            {
                Log.Error("CastEmptyBulletHelper PoolComponent is null");
                return;
            }

            GameObject effectGo = await poolComponent.GetEffect(assetPath);
            if (effectGo == null)
            {
                Log.Error($"CastEmptyBulletHelper GetEffect failed, assetPath={assetPath}");
                return;
            }

            EffectConfig effectConfig = EffectConfigCategory.Instance.Get(effectConfigId);
            UnitComponent unitComponent = scene.GetComponent<UnitComponent>();

            Unit bulletUnit = unitComponent.AddChild<Unit, int, string>(EmptyBulletUnitConfigId, "EmptyBullet");
            bulletUnit.OwnerId = caster.Id;
            bulletUnit.Position = caster.Position;

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

            float3 offset = target.Position - bulletUnit.Position;
            offset.y = 0;
            float distance = math.max(math.length(offset), 0.01f);
            float speed = distance / (flyTimeMs / 1000f);

            FollowComponent followComponent = bulletUnit.AddComponent<FollowComponent>();
            followComponent.Target = target;
            followComponent.Speed = speed;
            followComponent.EndTime = TimeInfo.Instance.ClientFrameTime() + flyTimeMs;
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
