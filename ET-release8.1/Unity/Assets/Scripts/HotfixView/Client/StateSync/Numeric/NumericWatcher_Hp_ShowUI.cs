namespace ET.Client
{
    /// <summary>
    /// 客户端监视 Hp 数值变化，更新 HUD 血条。
    /// </summary>
    [NumericWatcher(SceneType.Current, NumericType.Hp)]
    public class NumericWatcher_Hp_ShowUI : INumericWatcher
    {
        public void Run(Unit unit, NumbericChange args)
        {
            unit.GetComponent<UnitTopUIComponent>()?.RefreshHpBar();
        }
    }
}
