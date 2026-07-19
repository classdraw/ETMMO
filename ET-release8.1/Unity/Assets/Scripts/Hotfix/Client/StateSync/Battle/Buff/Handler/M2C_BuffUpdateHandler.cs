namespace ET.Client
{
    [MessageHandler(SceneType.StateSync)]
    public class M2C_BuffUpdateHandler: MessageHandler<Scene,M2C_BuffUpdate>
    {
        protected override async ETTask Run(Scene root, M2C_BuffUpdate message)
        {
            Log.Console($" 玩家 {message.UnitId} buffUpdate Id {message.BuffData.Id} ConfigId {message.BuffData.ConfigId} ");
            UnitComponent unitComponent = root.GetComponent<UnitComponent>();
            if (unitComponent==null)
            {
                return;
            }
            Unit unit = unitComponent.Get(message.UnitId);
            if (unit==null||unit.IsDisposed)
            {
                return;
            }
            ClientBuff clientBuff = unit.GetComponent<ClientBuffComponent>()?.Get(message.BuffData.Id);
            if (clientBuff==null||clientBuff.IsDisposed)
            {
                return;
            }

            unit.GetComponent<ClientBuffComponent>()?.Update(message.BuffData);
            BuffUpdate buffUpdate= new BuffUpdate();
            buffUpdate.Unit = unit;
            buffUpdate.BuffId = message.BuffData.Id;
            buffUpdate.BuffConfigId = message.BuffData.ConfigId;
            EventSystem.Instance.Publish(root,buffUpdate);
            
            await ETTask.CompletedTask;

        }
    }
}