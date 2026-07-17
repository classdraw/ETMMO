namespace ET.Server
{
    [MessageHandler(SceneType.Map)]
    public class C2M_CastInputHandler: MessageLocationHandler<Unit, C2M_CastInput, M2C_CastInput>
    {
        protected override async ETTask Run(Unit unit, C2M_CastInput request, M2C_CastInput response)
        {
            unit.Stop(1);
            await ETTask.CompletedTask;
        }
    }
}
