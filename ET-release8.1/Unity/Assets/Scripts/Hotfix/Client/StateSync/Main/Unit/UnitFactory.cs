using Unity.Mathematics;

namespace ET.Client
{
    public static partial class UnitFactory
    {
        public static Unit Create(Scene currentScene, UnitInfo unitInfo)
        {
	        UnitComponent unitComponent = currentScene.GetComponent<UnitComponent>();
	        Unit unit = unitComponent.AddChildWithId<Unit, int,string>(unitInfo.UnitId, unitInfo.ConfigId,unitInfo.Name);
	        unitComponent.Add(unit);
	        
	        unit.Position = unitInfo.Position;
	        unit.Forward = unitInfo.Forward;
	        unit.OwnerId = unitInfo.OwnerId;
	        unit.TeamId = unitInfo.TeamId;
	        unit.MapId = unitInfo.MapId;
	        if (unit.MapId == 0)
	        {
		        unit.MapId = MapConfigHelper.GetIdByLogicName(currentScene.Name);
	        }
	        
	        NumericComponent numericComponent = unit.AddComponent<NumericComponent>();

			foreach (var kv in unitInfo.KV)
			{
				numericComponent.Set(kv.Key, kv.Value);
			}
	        
	        unit.AddComponent<MoveComponent>();
	        if (unitInfo.MoveInfo != null)
	        {
		        if (unitInfo.MoveInfo.Points.Count > 0)
				{
					unitInfo.MoveInfo.Points[0] = unit.Position;
					unit.MoveToAsync(unitInfo.MoveInfo.Points).Coroutine();
				}
	        }

	        unit.AddComponent<ObjectWait>();

	        unit.AddComponent<XunLuoPathComponent>();
	        unit.AddComponent<ClientBuffComponent>();//buff管理器
	        unit.AddComponent<ClientCastComponent>();//cast管理器
	        
	        EventSystem.Instance.Publish(unit.Scene(), new AfterUnitCreate() {Unit = unit});
            return unit;
        }

        private const int EmptyBulletUnitConfigId = 9001;

        public static Unit CreateEmptyBullet(Scene scene, Unit caster)
        {
            UnitComponent unitComponent = scene.GetComponent<UnitComponent>();
            Unit bulletUnit = unitComponent.AddChild<Unit, int, string>(EmptyBulletUnitConfigId, "EmptyBullet");
            bulletUnit.OwnerId = caster.Id;
            bulletUnit.Position = caster.Position;
            bulletUnit.AddComponent<FollowComponent>();
            return bulletUnit;
        }
    }
}
