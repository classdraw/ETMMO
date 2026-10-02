namespace ET.Client
{
	[MessageHandler(SceneType.StateSync)]
	public class M2C_CreateUnitsHandler: MessageHandler<Scene, M2C_CreateUnits>
	{
		protected override async ETTask Run(Scene root, M2C_CreateUnits message)
		{
			Scene currentScene = root.CurrentScene();
			UnitComponent unitComponent = currentScene.GetComponent<UnitComponent>();
			
			foreach (UnitInfo unitInfo in message.Units)
			{
				if (unitComponent.Get(unitInfo.UnitId) != null)
				{
					continue;
				}
				if (unitInfo.Type == (int)UnitType.Robot)
				{
					Log.Info($"[Robot][Client] M2C_CreateUnits 收到机器人 unitId={unitInfo.UnitId} name={unitInfo.Name} mapType={unitInfo.Type} scene={currentScene.Name}");
				}

				Unit unit = UnitFactory.Create(currentScene, unitInfo);
			}
			await ETTask.CompletedTask;
		}
	}
}
