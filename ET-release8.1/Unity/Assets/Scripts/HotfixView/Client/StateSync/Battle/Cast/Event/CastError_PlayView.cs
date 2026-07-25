namespace ET.Client
{
    //技能释放失败
    [Event(SceneType.Current)]
    public class CastError_PlayView:AEvent<Scene,CastError>
    {
        protected override async ETTask Run(Scene scene, CastError args)
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