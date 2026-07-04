namespace ET.Client
{
    [MessageHandler(SceneType.StateSync)]
    public class M2C_BuffUpdateHandler: MessageHandler<Scene,M2C_BuffUpdate>
    {
        protected override async ETTask Run(Scene root, M2C_BuffUpdate message)
        {
            Log.Console($" 玩家 {message.UnitId} buffUpdate Id {message.BuffData.Id} ConfigId {message.BuffData.ConfigId} ");
            BuffUpdate buffUpdate= new BuffUpdate();
            buffUpdate.Unit = root.GetComponent<UnitComponent>().Get(message.UnitId);
            buffUpdate.BuffId = message.BuffData.Id;

            EventSystem.Instance.Publish(root,buffUpdate);
            await ETTask.CompletedTask;
        }
    }
}