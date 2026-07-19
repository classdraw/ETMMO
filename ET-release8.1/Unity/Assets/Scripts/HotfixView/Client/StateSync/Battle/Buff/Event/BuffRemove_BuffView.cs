namespace ET.Client
{
    //buff移除
    [Event(SceneType.Current)]
    public class BuffRemove_BuffView:AEvent<Scene,BuffRemove>
    {
        protected override async ETTask Run(Scene scene, BuffRemove args)
        {
            Unit unit = args.Unit;
            if (unit == null || unit.IsDisposed)
            {
                return;
            }

            MountComponent mountComponent = unit.GetComponent<MountComponent>();
            if (mountComponent == null || mountComponent.IsDisposed)
            {
                return;
            }

            mountComponent.RemoveEffectByBuffId(args.BuffId);
            await ETTask.CompletedTask;
        }
    }
}
