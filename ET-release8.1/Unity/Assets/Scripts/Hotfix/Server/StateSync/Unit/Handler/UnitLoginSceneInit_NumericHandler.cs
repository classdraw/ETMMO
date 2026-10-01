namespace ET.Server
{
    [FriendOf(typeof(NumericComponent))]
    [FriendOf(typeof(UnitDBSaveComponent))]
    [Event(SceneType.All)]
    public class UnitLoginSceneInit_NumericHandler : AEvent<Scene, UnitLoginSceneInit>
    {
        protected override async ETTask Run(Scene scene, UnitLoginSceneInit args)
        {
            Unit unit = args.Unit;
            if (unit == null || unit.IsDisposed)
            {
                return;
            }

            NumericComponent numeric = unit.GetComponent<NumericComponent>();
            if (numeric == null)
            {
                return;
            }

            if (UnitLoginSceneSerializeDefaults.ResetTransientNumeric(numeric))
            {
                unit.GetComponent<UnitDBSaveComponent>()?.AddChange(typeof(NumericComponent));
            }

            await ETTask.CompletedTask;
        }
    }
}
