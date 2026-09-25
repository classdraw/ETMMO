namespace ET.Client
{
    [MessageHandler(SceneType.StateSync)]
    public class M2C_SetPositionHandler : MessageHandler<Scene, M2C_SetPosition>
    {
        protected override async ETTask Run(Scene root, M2C_SetPosition message)
        {
            Unit unit = root.CurrentScene().GetComponent<UnitComponent>().Get(message.UnitId);
            if (unit == null)
            {
                return;
            }

            unit.GetComponent<MoveComponent>()?.Stop(true);
            unit.Position = message.Position;
            unit.Rotation = message.Rotation;
            await ETTask.CompletedTask;
        }
    }
}
