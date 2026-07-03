namespace ET.Server
{
    // 进入视野通知
    [Event(SceneType.Map)]
    public class UnitEnterSightRange_NotifyClient: AEvent<Scene, UnitEnterSightRange>
    {
        protected override async ETTask Run(Scene scene, UnitEnterSightRange args)
        {
            if (scene == null || scene.IsDisposed)
            {
                return;
            }

            AOIEntity a = args.A;
            AOIEntity b = args.B;
            if (a == null || a.IsDisposed || b == null || b.IsDisposed)
            {
                return;
            }

            if (a.Id == b.Id)
            {
                return;
            }

            Unit ua = a.GetParent<Unit>();
            if (ua == null || ua.IsDisposed || ua.Type() != UnitType.Player)
            {
                return;
            }

            Unit ub = b.GetParent<Unit>();
            if (ub == null || ub.IsDisposed)
            {
                return;
            }

            MapMessageHelper.NoticeUnitAdd(ua, ub);
            
            await ETTask.CompletedTask;
        }
    }
}