using Unity.Mathematics;

namespace ET.Server
{
    [Actions(ActionsType.CastBullet)]
    [FriendOf(typeof(Actions))]
    [FriendOf(typeof(Cast))]
    [FriendOf(typeof(BulletComponent))]
    public class Actions_CastBullet : IActions
    {
        public void Run(Actions actions, ActionsRunType actionsRunType)
        {
            Cast cast = actions.CastSelf;
            if (cast == null || actionsRunType != ActionsRunType.CastHit)
            {
                return;
            }

            Unit caster = actions.Caster;
            if (caster == null || caster.IsDisposed || !caster.IsBattleUnit())
            {
                return;
            }

            ActionsConfig config = actions.Config;
            if (config.ActionsParam == null || config.ActionsParam.Length < 1)
            {
                Log.Error($"Actions_CastBullet ActionsParam invalid: configId={config.Id}");
                return;
            }

            int bulletConfigId = config.ActionsParam[0];
            if (!BulletConfigCategory.Instance.Contain(bulletConfigId))
            {
                Log.Error($"Actions_CastBullet BulletConfig not found: configId={config.Id}, bulletConfigId={bulletConfigId}");
                return;
            }

            CastBulletSpawnOrigin spawnOrigin = CastBulletSpawnOrigin.Caster;
            if (config.ActionsParam.Length >= 2)
            {
                spawnOrigin = (CastBulletSpawnOrigin)config.ActionsParam[1];
            }

            BulletConfig bulletConfig = BulletConfigCategory.Instance.Get(bulletConfigId);
            CreateBulletFromCast(actions, caster, bulletConfig, spawnOrigin);
        }

        private static void CreateBulletFromCast(Actions actions, Unit caster, BulletConfig bulletConfig, CastBulletSpawnOrigin spawnOrigin)
        {
            Cast cast = actions.CastSelf;
            float3 spawnPos = ResolveBulletSpawnPosition(actions, caster, spawnOrigin);
            Unit bullet = UnitFactory.CreateBullet(actions.Scene(), caster.Id, bulletConfig, spawnPos, caster.Rotation);
            BulletComponent bulletComponent = bullet?.GetComponent<BulletComponent>();
            if (bulletComponent == null)
            {
                return;
            }

            bulletComponent.InputUnitId = cast.InputUnitId;
            bulletComponent.InputPos = cast.InputPos;
            bulletComponent.Start();
        }

        /// <summary>
        /// ActionsParam[1]=0 施法者位置；=1 优先 InputUnit 坐标，否则 InputPos；无效时回退施法者位置。
        /// </summary>
        private static float3 ResolveBulletSpawnPosition(Actions actions, Unit caster, CastBulletSpawnOrigin spawnOrigin)
        {
            if (spawnOrigin == CastBulletSpawnOrigin.Caster)
            {
                return caster.Position;
            }

            Cast cast = actions.CastSelf;
            if (cast == null)
            {
                return caster.Position;
            }

            Unit inputUnit = GetInputUnit(actions, cast.InputUnitId);
            if (inputUnit != null)
            {
                return inputUnit.Position;
            }

            float3 inputPos = cast.InputPos;
            if (math.lengthsq(inputPos) > 0.001f)
            {
                return inputPos;
            }

            Log.Warning($"Actions_CastBullet spawnOrigin=Input 但无有效 InputUnit/InputPos，回退施法者位置: configId={actions.Config.Id}");
            return caster.Position;
        }

        private static Unit GetInputUnit(Actions actions, long inputUnitId)
        {
            if (inputUnitId == 0)
            {
                return null;
            }

            Unit inputUnit = actions.Scene()?.GetComponent<UnitComponent>()?.Get(inputUnitId);
            if (inputUnit == null || inputUnit.IsDisposed || !inputUnit.IsBattleUnit())
            {
                return null;
            }

            return inputUnit;
        }
    }
}
