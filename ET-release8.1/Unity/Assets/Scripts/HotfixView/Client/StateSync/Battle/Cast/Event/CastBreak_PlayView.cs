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