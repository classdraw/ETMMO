namespace ET.Client
{
    [MessageHandler(SceneType.StateSync)]
    public class M2C_CastInputHandler: MessageHandler<Scene, M2C_CastInput>
    {
        protected override async ETTask Run(Scene root, M2C_CastInput message)
        {
            await ETTask.CompletedTask;
        }
    }
}
