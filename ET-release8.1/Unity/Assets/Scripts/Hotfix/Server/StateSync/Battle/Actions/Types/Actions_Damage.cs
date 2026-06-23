namespace ET.Server
{
    [Actions(ActionsType.NumericChange)]
    [FriendOf(typeof(Actions))]
    [FriendOf(typeof(Cast))]
    public class Actions_Damage:IActions
    {
        public void Run(Actions actions, ActionsRunType actionsRunType)
        {
            Cast cast = actions.CastSelf;
            if (cast==null||actionsRunType!=ActionsRunType.CastHit)
            {
                return;
            }

            Unit caster = cast.Caster;
            if (cast.Targets.Count<=0||caster==null||caster.IsDisposed)
            {
                return;
            }
            
            UnitComponent unitComponent = cast.Scene().GetComponent<UnitComponent>();
            foreach (var unitId in cast.Targets)
            {
                Unit target = unitComponent.Get(unitId);
                if (target==null||target.IsDisposed)
                {
                    continue;
                }
                BattleHelper.CalcAttack(caster,target, actions);
            }
        }
    }
}

