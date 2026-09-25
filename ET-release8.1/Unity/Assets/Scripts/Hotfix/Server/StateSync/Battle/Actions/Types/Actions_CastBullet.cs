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

            BulletConfig bulletConfig = BulletConfigCategory.Instance.Get(bulletConfigId);
            CreateBulletFromCast(actions, caster, bulletConfig);
            
        }

        private static void CreateBulletFromCast(Actions actions, Unit caster, BulletConfig bulletConfig)
        {
            Cast cast = actions.CastSelf;
            Unit bullet = UnitFactory.CreateBullet(actions.Scene(), caster.Id, bulletConfig, caster.Position,
                caster.Rotation);
            BulletComponent bulletComponent = bullet?.GetComponent<BulletComponent>();
            if (bulletComponent == null)
            {
                return;
            }

            bulletComponent.InputUnitId = cast.InputUnitId;
            bulletComponent.InputPos = cast.InputPos;
            bulletComponent.Start();
        }
    }
}
