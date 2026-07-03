namespace ET.Server
{
    // 离开视野
    [Event(SceneType.Map)]
    public class UnitLeaveSightRange_NotifyClient: AEvent<Scene, UnitLeaveSightRange>
    {
        protected override async ETTask Run(Scene scene, UnitLeaveSightRange args)
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

            if (a.Unit.Type() != UnitType.Player)
            {
                return;
            }

            Unit player = a.GetParent<Unit>();
            Unit leaveUnit = b.GetParent<Unit>();
            if (player == null || player.IsDisposed || leaveUnit == null || leaveUnit.IsDisposed)
            {
                return;
            }

            MapMessageHelper.NoticeUnitRemove(player, leaveUnit);
            await ETTask.CompletedTask;
        }
    }
}