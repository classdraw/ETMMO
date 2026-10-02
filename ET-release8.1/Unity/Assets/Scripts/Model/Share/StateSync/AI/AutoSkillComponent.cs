namespace ET
{
    [ComponentOf(typeof(Unit))]
    public class AutoSkillComponent : Entity, IAwake, IDestroy
    {
        public long NextAttackTime;
    }
}
