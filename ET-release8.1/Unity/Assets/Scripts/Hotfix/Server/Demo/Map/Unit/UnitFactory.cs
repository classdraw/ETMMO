using System;
using Unity.Mathematics;

namespace ET.Server
{
    public static partial class UnitFactory
    {
        public static Unit  Create(Scene scene, long id,int baseAvatar, UnitType unitType)
        {
            UnitComponent unitComponent = scene.GetComponent<UnitComponent>();
            switch (unitType)
            {
                case UnitType.Player:
                {
                    Unit unit = unitComponent.AddChildWithId<Unit, int, int>(id, 1001,baseAvatar);
                    unit.AddComponent<MoveComponent>();
                    unit.Position = new float3(-10, 0, -10);
                    NumericComponent numericComponent = unit.AddComponent<NumericComponent>();
                    numericComponent.Set(NumericType.Speed, 3f); // 速度是3米每秒
                    numericComponent.Set(NumericType.AOI, 6000); // 视野15米
                    
                    unitComponent.Add(unit);
                    // 加入aoi
                    var aoiEntity=unit.AddComponent<AOIEntity, int, float3>(6 * 1000, unit.Position);
                    return unit;
                }
                default:
                    throw new Exception($"not such unit type: {unitType}");
            }
        }
    }
}