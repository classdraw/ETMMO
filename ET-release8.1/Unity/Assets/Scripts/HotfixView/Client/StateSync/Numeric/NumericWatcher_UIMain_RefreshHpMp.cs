namespace ET.Client
{
    internal static class UIMainHpMpNumericWatcherHelper
    {
        public static void RefreshIfMyUnit(Unit unit)
        {
            if (unit == null || unit.IsDisposed)
            {
                return;
            }

            Scene currentScene = unit.Scene();
            if (currentScene == null || !unit.IsMyUnit(currentScene))
            {
                return;
            }

            UIComponent uiComponent = currentScene.GetComponent<UIComponent>();
            if (uiComponent == null)
            {
                return;
            }

            UI mainUi = uiComponent.Get(UIType.UIMain);
            mainUi?.GetComponent<UIMainComponent>()?.RefreshHpMpSlider();
        }
    }

    [NumericWatcher(SceneType.Current, NumericType.Hp)]
    public class NumericWatcher_UIMain_Hp : INumericWatcher
    {
        public void Run(Unit unit, NumbericChange args)
        {
            UIMainHpMpNumericWatcherHelper.RefreshIfMyUnit(unit);
        }
    }

    [NumericWatcher(SceneType.Current, NumericType.MaxHp)]
    public class NumericWatcher_UIMain_MaxHp : INumericWatcher
    {
        public void Run(Unit unit, NumbericChange args)
        {
            UIMainHpMpNumericWatcherHelper.RefreshIfMyUnit(unit);
        }
    }

    [NumericWatcher(SceneType.Current, NumericType.Sp)]
    public class NumericWatcher_UIMain_Sp : INumericWatcher
    {
        public void Run(Unit unit, NumbericChange args)
        {
            UIMainHpMpNumericWatcherHelper.RefreshIfMyUnit(unit);
        }
    }

    [NumericWatcher(SceneType.Current, NumericType.MaxSp)]
    public class NumericWatcher_UIMain_MaxSp : INumericWatcher
    {
        public void Run(Unit unit, NumbericChange args)
        {
            UIMainHpMpNumericWatcherHelper.RefreshIfMyUnit(unit);
        }
    }
}
