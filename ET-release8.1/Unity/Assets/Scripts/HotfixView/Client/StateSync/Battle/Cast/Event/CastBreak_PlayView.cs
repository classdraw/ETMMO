namespace ET.Client
{
    //技能打断结束
    [Event(SceneType.Current)]
    public class CastBreak_PlayView:AEvent<Scene,CastBreak>
    {
        protected override async ETTask Run(Scene scene, CastBreak args)
        {
            Unit unit = scene.GetComponent<UnitComponent>().Get(args.CasterId);
            if (unit==null||unit.IsDisposed)
            {
                return;
            }
            //后面走配置表
            unit.GetComponent<AnimatorComponent>().Play(MotionType.Idle,1f);

            await ETTask.CompletedTask;
        }
    }
}