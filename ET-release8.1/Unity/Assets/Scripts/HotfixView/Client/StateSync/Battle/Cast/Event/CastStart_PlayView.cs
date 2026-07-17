namespace ET.Client
{
    //技能开始释放逻辑
    [Event(SceneType.Current)]
    public class CastStart_PlayView:AEvent<Scene,CastStart>
    {
        protected override async ETTask Run(Scene scene, CastStart args)
        {
            Unit unit = scene.GetComponent<UnitComponent>().Get(args.CasterId);
            if (unit==null||unit.IsDisposed)
            {
                return;
            }
            //后面走配置表
            unit.GetComponent<AnimatorComponent>().PlayInTime(MotionType.Attack,0.5f);

            await ETTask.CompletedTask;
        }
    }
}

