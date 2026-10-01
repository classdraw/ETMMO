namespace ET.Server
{
    /// <summary>
    /// 登录 / 创角进首场景：统一入口，重置 UnitCache 中误存的运行时数据。
    /// </summary>
    public static class UnitLoginSceneInitHelper
    {
        /// <param name="persistToCache">为 true 时全量写回 UnitCache（Gate 读档后纠正脏数据）。</param>
        public static void Apply(Unit unit, bool persistToCache)
        {
            if (unit == null || unit.IsDisposed)
            {
                return;
            }

            Scene scene = unit.Scene();
            if (scene == null)
            {
                return;
            }

            EventSystem.Instance.Publish(scene, new UnitLoginSceneInit { Unit = unit });

            if (persistToCache)//目前只有登录的时候是true
            {
                UnitCacheHelper.AddOrUpdateUnitAllCache(unit);
            }
        }
    }
}
