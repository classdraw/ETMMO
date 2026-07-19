namespace ET.Client
{
    [MessageHandler(SceneType.StateSync)]
    public class M2C_BuffRemoveHandler: MessageHandler<Scene,M2C_BuffRemove>
    {
        protected override async ETTask Run(Scene root, M2C_BuffRemove message)
        {
            Log.Console($" 玩家 {message.UnitId} 移除buff Id {message.BuffId} ");
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
            if (clientBuff == null||clientBuff.IsDisposed)
            {
                return;
            }

            BuffRemove buffRemove = new BuffRemove();
            buffRemove.Unit = unit;
            buffRemove.BuffId = message.BuffId;
            EventSystem.Instance.Publish(currentScene, buffRemove);
            
            unit.GetComponent<ClientBuffComponent>()?.Remove(message.BuffId);
            await ETTask.CompletedTask;
        }
    }
}
