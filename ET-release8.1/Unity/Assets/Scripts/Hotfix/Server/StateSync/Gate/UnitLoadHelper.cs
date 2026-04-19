namespace ET.Server
{
    public static class UnitLoadHelper
    {
        public static async ETTask<(bool,Unit)> LoadUnit(Player player)
        {
            // 在Gate上动态创建一个Map Scene，把Unit从DB中加载放进来，然后传送到真正的Map中，这样登陆跟传送的逻辑就完全一样了
            GateMapComponent gateMapComponent = player.AddComponent<GateMapComponent>();
            gateMapComponent.Scene = await GateMapFactory.Create(gateMapComponent, player.Id, IdGenerater.Instance.GenerateInstanceId(), "GateMap");
            Unit unit =await UnitCacheHelper.GetUnitCache(player.Root(), gateMapComponent.Scene, player.UnitId);
            bool isNewUnit = unit == null;
            if (isNewUnit)
            {
                unit=UnitFactory.Create(gateMapComponent.Scene, player.Id,player.BaseAvatar, UnitType.Player);
                unit.AddComponent<UnitDBSaveComponent>();
                UnitCacheHelper.AddOrUpdateUnitAllCache(unit);//新角色把这个角色身上所有组件更新到缓存和数据库
            }
            else
            {
                if (unit.GetComponent<UnitDBSaveComponent>()==null)
                {
                    unit.AddComponent<UnitDBSaveComponent>();
                }
            }

            return (isNewUnit,unit);
        }
    }
}

