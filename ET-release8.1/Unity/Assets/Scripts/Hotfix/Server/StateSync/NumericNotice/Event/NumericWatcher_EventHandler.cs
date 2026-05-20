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
}

