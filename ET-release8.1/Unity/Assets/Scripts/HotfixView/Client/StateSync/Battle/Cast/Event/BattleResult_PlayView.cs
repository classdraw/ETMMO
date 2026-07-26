namespace ET.Client
{
    [Event(SceneType.Current)]
    public class BattleResult_PlayView : AEvent<Scene, BattleResult>
    {
        protected override async ETTask Run(Scene scene, BattleResult args)
        {
            Unit target = scene.GetComponent<UnitComponent>()?.Get(args.TargetId);
            if (target == null || target.IsDisposed)
            {
                return;
            }

            BattleHudHelper.ShowBattleResultNumber(target, args.Damage, args.IsCrit);
            await ETTask.CompletedTask;
        }
    }
}
