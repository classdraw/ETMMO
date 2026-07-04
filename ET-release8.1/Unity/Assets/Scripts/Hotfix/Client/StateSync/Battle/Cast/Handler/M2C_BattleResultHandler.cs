namespace ET.Client
{
    [MessageHandler(SceneType.StateSync)]
    public class M2C_BattleResultHandler: MessageHandler<Scene,M2C_BattleResult>
    {
        protected override async ETTask Run(Scene root, M2C_BattleResult message)
        {

            await ETTask.CompletedTask;
            
        }
    }
}