namespace ET.Server
{
    [FriendOf(typeof(SkillStatusComponent))]
    [FriendOf(typeof(UnitDBSaveComponent))]
    [Event(SceneType.All)]
    public class UnitLoginSceneInit_SkillStatusHandler : AEvent<Scene, UnitLoginSceneInit>
    {
        protected override async ETTask Run(Scene scene, UnitLoginSceneInit args)
        {
            Unit unit = args.Unit;
            if (unit == null || unit.IsDisposed)
            {
                return;
            }

            SkillStatusComponent skillStatus = unit.GetComponent<SkillStatusComponent>();
            if (skillStatus == null)
            {
                return;
            }

            if (UnitLoginSceneSerializeDefaults.ResetTransientSkillStatus(skillStatus))
            {
                unit.GetComponent<UnitDBSaveComponent>()?.AddChange(typeof(SkillStatusComponent));
            }

            await ETTask.CompletedTask;
        }
    }
}
