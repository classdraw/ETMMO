namespace ET.Client
{
    [MessageHandler(SceneType.StateSync)]
    public class M2C_BuffAddHandler: MessageHandler<Scene,M2C_BuffAdd>
    {
        protected override async ETTask Run(Scene root, M2C_BuffAdd message)
        {
            Log.Console($" 玩家 {message.UnitId} 新增buff Id {message.BuffData.Id} ConfigId {message.BuffData.ConfigId} ");
            BuffAdd buffAdd = new BuffAdd();
            buffAdd.Unit = root.GetComponent<UnitComponent>().Get(message.UnitId);
            buffAdd.BuffId = message.BuffData.Id;
            buffAdd.BuffConfigId = message.BuffData.ConfigId;
            EventSystem.Instance.Publish(root,buffAdd);
            await ETTask.CompletedTask;
        }
    }
}

