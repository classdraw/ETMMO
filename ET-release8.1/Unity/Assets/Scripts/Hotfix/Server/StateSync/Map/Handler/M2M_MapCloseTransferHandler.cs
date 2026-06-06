namespace ET.Server
{
    [MessageHandler(SceneType.Map)]
    public class M2M_MapCloseTransferHandler : MessageHandler<Scene, M2M_MapCloseTransfer>
    {
        protected override async ETTask Run(Scene scene, M2M_MapCloseTransfer message)
        {
            UnitComponent unitComponent = scene.GetComponent<UnitComponent>();
            using var unitList = ListComponent<Unit>.Create();
            foreach (Entity entity in unitComponent.Children.Values)
            {
                if (entity is Unit unit && unit.IsPlayer())
                {
                    unitList.Add(unit);
                }
            }

            foreach (Unit unit in unitList)
            {
                TransferHelper.TransferAtFrameFinish(unit, message.TargetActorId, message.MapConfigId).Coroutine();
            }

            await ETTask.CompletedTask;
        }
    }
}
