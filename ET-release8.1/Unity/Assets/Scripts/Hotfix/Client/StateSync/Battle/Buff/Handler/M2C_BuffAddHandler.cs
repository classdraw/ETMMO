namespace ET.Client
{
    [MessageHandler(SceneType.StateSync)]
    public class M2C_BuffAddHandler: MessageHandler<Scene,M2C_BuffAdd>
    {
        protected override async ETTask Run(Scene root, M2C_BuffAdd message)
        {
            if (message.BuffData == null)
            {
                return;
            }

            Log.Console($" 玩家 {message.UnitId} 新增buff Id {message.BuffData.Id} ConfigId {message.BuffData.ConfigId} ");
            Scene currentScene = root.CurrentScene();
            UnitComponent unitComponent = currentScene?.GetComponent<UnitComponent>();
            if (unitComponent == null)
            {
                return;
            }

            Unit unit = unitComponent.Get(message.UnitId);
            if (unit == null || unit.IsDisposed)
            {
                return;
            }
            
            var buff=unit.CreateAndAddBuff(message.BuffData);

            BuffAdd buffAdd = new BuffAdd();
            buffAdd.Unit = unit;
            buffAdd.BuffId = message.BuffData.Id;
            buffAdd.BuffConfigId = message.BuffData.ConfigId;
            EventSystem.Instance.Publish(currentScene, buffAdd);
            await ETTask.CompletedTask;
        }
    }
}
