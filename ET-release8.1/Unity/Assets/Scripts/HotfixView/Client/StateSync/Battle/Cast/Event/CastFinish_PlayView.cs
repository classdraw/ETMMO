namespace ET.Client
{
    //技能结束
    [Event(SceneType.Current)]
    public class CastFinish_PlayView:AEvent<Scene,CastFinish>
    {
        protected override async ETTask Run(Scene scene, CastFinish args)
        {
            Unit unit = scene.GetComponent<UnitComponent>().Get(args.CasterId);
            if (unit==null||unit.IsDisposed)
            {
                return;
            }
            ClientCast clientCast = unit.GetComponent<ClientCastComponent>().Get(args.CastId);
            if (clientCast==null||clientCast.IsDisposed)
            {
                return;
            }

            //后面走配置表
            unit.GetComponent<Animator2DComponent>()?.Play(MotionType.Idle,1f);

            await ETTask.CompletedTask;
        }
    }
}