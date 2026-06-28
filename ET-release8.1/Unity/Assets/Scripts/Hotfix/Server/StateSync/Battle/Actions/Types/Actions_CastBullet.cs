namespace ET.Server
{
    [Actions(ActionsType.CastBullet)]
    [FriendOf(typeof(Actions))]
    [FriendOf(typeof(Cast))]
    public class Actions_CastBullet:IActions
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

            Unit target = actions.Owner;
            if (target == null || target.IsDisposed || !target.IsBattleUnit())
            {
                return;
            }

            ActionsConfig config = actions.Config;
            if (config.ActionsParam == null || config.ActionsParam.Length < 2)
            {
                Log.Error($"Actions_CastBullet ActionsParam invalid: configId={config.Id}");
                return;
            }

            int unitConfigId = config.ActionsParam[0];
            int bulletConfigId = config.ActionsParam[1];
            Unit bullet = UnitFactory.CreateBullet(actions.Scene(), caster.Id, unitConfigId, bulletConfigId, caster.Position);
            if (bullet == null)
            {
                return;
            }

            bullet.GetComponent<BulletComponent>()?.Start();
        }
    }
}
