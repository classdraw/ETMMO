namespace ET.Client
{
    [MessageHandler(SceneType.StateSync)]
    public class M2C_BuffTickHandler: MessageHandler<Scene,M2C_BuffTick>
    {
        protected override async ETTask Run(Scene root, M2C_BuffTick message)
        {
            Log.Console($" 玩家 {message.UnitId} buffTick Id {message.BuffId} ");
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

            ClientBuff clientBuff = unit.GetComponent<ClientBuffComponent>()?.Get(message.BuffId);
            if (clientBuff == null || clientBuff.IsDisposed)
            {
                return;
            }

            BuffTick buffTick = new BuffTick();
            buffTick.Unit = unit;
            buffTick.BuffId = message.BuffId;

            EventSystem.Instance.Publish(currentScene, buffTick);
            await ETTask.CompletedTask;
        }
    }
}