namespace ET.Client
{
    [MessageHandler(SceneType.StateSync)]
    public class M2C_CastStartHandler: MessageHandler<Scene,M2C_CastStart>
    {
        protected override async ETTask Run(Scene root, M2C_CastStart message)
        {
            Log.Console($"玩家 {message.CasterId} 开始释放 {message.CastConfigId} 技能 {message.CastId} ！！！");
            //技能释放流程
            CastStart castStart = new CastStart();
            castStart.CastId = message.CastId;
            castStart.CasterId = message.CasterId;
            castStart.CasterConfigId = message.CastConfigId;
            EventSystem.Instance.Publish(root,castStart);
            await ETTask.CompletedTask;
            
        }
    }
}

