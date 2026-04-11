namespace ET.Server
{
    public static class UnitLoadHelper
    {
        public static async ETTask<(bool,Unit)> LoadUnit(Player player)
        {
            // 在Gate上动态创建一个Map Scene，把Unit从DB中加载放进来，然后传送到真正的Map中，这样登陆跟传送的逻辑就完全一样了
            GateMapComponent gateMapComponent = player.AddComponent<GateMapComponent>();
            gateMapComponent.Scene = await GateMapFactory.Create(gateMapComponent, player.Id, IdGenerater.Instance.GenerateInstanceId(), "GateMap");
            Unit unit = null;//UnitFactory.Create(scene, player.Id, UnitType.Player);
            bool isNewUnit = unit == null;
            if (isNewUnit)
            {
                unit=UnitFactory.Create(gateMapComponent.Scene, player.Id, UnitType.Player);
                //unit.AddComponent<UnitDBComponent>();
                //unitCacheHelper.AddOrUpdateUnitAllCache(unit);
            }
            else
            {
                //if (unit.GetComponent<UnitDBComponent>()==null)
                //{
                  //  unit.AddComponent<UnitDBComponent>();
                //}
            }

            return (isNewUnit,unit);
        }
    }
}

