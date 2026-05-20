namespace ET.Server
{
    [NumericWatcher(SceneType.Map,NumericType.Level)] // 等级
    public class NumericWatcher_Level : INumericWatcher
    {
        public void Run(Unit unit,NumbericChange args)
        {
            RankHelper.AddOrUpdateLevelRank(unit);
        }
    }
    
    /**
     *
     *
     *    [NumericWatcher(SceneType.Map,NumericType.Agility)] // 敏捷
    public class NumericWatcher_EventHandler : INumericWatcher
    {
        public void Run(Unit unit,NumbericChange args)
        {
            NumericComponent numericComponent = unit.GetComponent<NumericComponent>();
            numericComponent[NumericType.DefenseBase] =  numericComponent[NumericType.Agility]/10;
            numericComponent[NumericType.CombatPower] += 10;
        }
    }
     * 
     */
}

