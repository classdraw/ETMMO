namespace ET.Server
{
    public static class UnitLoadHelper
    {
        public static async ETTask<(bool, Unit)> LoadUnit(Player player)
        {
            GateMapComponent gateMapComponent = player.AddComponent<GateMapComponent>();
            gateMapComponent.Scene = await GateMapFactory.Create(gateMapComponent, player.Id, IdGenerater.Instance.GenerateInstanceId(), "GateMap");
            Unit unit = await UnitCacheHelper.GetUnitCache(player.Root(), gateMapComponent.Scene, player.UnitId);
            bool isNewUnit = unit == null;
            if (isNewUnit)
            {
                unit = UnitFactory.Create(
                    gateMapComponent.Scene,
                    player.Id,
                    player.ConfigId,
                    player.Name,
                    UnitType.Player);
                player.ApplyProfileToUnit(unit);
                UnitCacheHelper.AddOrUpdateUnitAllCache(unit);
            }
            else
            {
                if (unit.GetComponent<UnitDBSaveComponent>() == null)
                {
                    unit.AddComponent<UnitDBSaveComponent>();
                }

                player.ApplyProfileToUnit(unit);

                if (unit.GetComponent<CastComponent>() == null)
                {
                    unit.AddComponent<CastComponent>();
                }

                if (unit.GetComponent<SkillStatusComponent>() == null)
                {
                    unit.AddComponent<SkillStatusComponent>();
                }

                if (unit.GetComponent<BuffComponent>() == null)
                {
                    unit.AddComponent<BuffComponent>();
                }
            }

            return (isNewUnit, unit);
        }
    }
}
