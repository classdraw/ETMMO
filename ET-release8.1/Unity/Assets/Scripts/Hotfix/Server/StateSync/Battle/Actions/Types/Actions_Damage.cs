namespace ET.Server
{
    [Actions(ActionsType.Damage)]
    [FriendOf(typeof(Actions))]
    [FriendOf(typeof(Cast))]
    public class Actions_Damage:IActions
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

            BattleHelper.CalcAttack(caster, target, actions);
        }
    }
}
