using Unity.Mathematics;

namespace ET.Server
{
    [FriendOf(typeof(MoveComponent))]
    [FriendOf(typeof(NumericComponent))]
    public static class UnitInfoHelper
    {
        public static UnitInfo CreateUnitInfo(Unit unit)
        {
            UnitInfo unitInfo = UnitInfo.Create();
            NumericComponent nc = unit.GetComponent<NumericComponent>();
            unitInfo.UnitId = unit.Id;
            unitInfo.Name = unit.Name;
            unitInfo.ConfigId = unit.ConfigId;
            unitInfo.TableConfigId = unit.TableConfigId;
            unitInfo.BaseExternalDisplay = unit.BaseExternalDisplay ?? string.Empty;
            unitInfo.Race = unit.Race;
            unitInfo.Gender = unit.Gender;
            unitInfo.Type = (int)unit.Type();
            unitInfo.OwnerId = unit.OwnerId;
            unitInfo.TeamId = unit.TeamId;
            unitInfo.Position = unit.Position;
            unitInfo.Forward = unit.Forward;
            unitInfo.MapId = unit.MapId;

            MoveComponent moveComponent = unit.GetComponent<MoveComponent>();
            if (moveComponent != null)
            {
                if (!moveComponent.IsArrived())
                {
                    unitInfo.MoveInfo = MoveInfo.Create();
                    unitInfo.MoveInfo.Points.Add(unit.Position);
                    for (int i = moveComponent.N; i < moveComponent.Targets.Count; ++i)
                    {
                        float3 pos = moveComponent.Targets[i];
                        unitInfo.MoveInfo.Points.Add(pos);
                    }
                }
            }

            if (nc != null && nc.NumericDic != null)
            {
                foreach ((int key, long value) in nc.NumericDic)
                {
                    unitInfo.KV.Add(key, value);
                }
            }

            return unitInfo;
        }
    }
}
