namespace ET.Client
{
    //技能结束
    [Event(SceneType.Current)]
    [FriendOfAttribute(typeof(ET.Client.Animator2DComponent))]
    public class CastFinish_PlayView : AEvent<Scene, CastFinish>
    {
        protected override async ETTask Run(Scene scene, CastFinish args)
        {
            Unit unit = scene.GetComponent<UnitComponent>().Get(args.CasterId);
            if (unit == null || unit.IsDisposed)
            {
                return;
            }
            ClientCast clientCast = unit.GetComponent<ClientCastComponent>().Get(args.CastId);
            if (clientCast == null || clientCast.IsDisposed)
            {
                return;
            }

            Animator2DComponent animator = unit.GetComponent<Animator2DComponent>();
            if (animator?.AnimPlayer != null && animator.AnimPlayer.ShouldReturnToIdleAfterCurrentClip())
            {
                return;
            }

            // 施法结束需强制回 Idle；此时尚未 Remove ClientCast，Play() 会被 IsCasting 拦截。
            if (animator?.AnimPlayer != null)
            {
                animator.SyncFacingFromUnit();
                animator.AnimPlayer.Play((int)MotionType.Idle, animator.Facing, 1f);
            }

            await ETTask.CompletedTask;
        }
    }
}