namespace ET.Client
{
    //技能打断结束
    [Event(SceneType.Current)]
    [FriendOfAttribute(typeof(ET.Client.Animator2DComponent))]
    public class CastBreak_PlayView : AEvent<Scene, CastBreak>
    {
        protected override async ETTask Run(Scene scene, CastBreak args)
        {
            Unit unit = scene.GetComponent<UnitComponent>().Get(args.CasterId);
            if (unit == null || unit.IsDisposed)
            {
                return;
            }

            ClientCastComponent clientCastComponent = unit.GetComponent<ClientCastComponent>();
            ClientCast clientCast = clientCastComponent.Get(args.CastId);
            if (clientCast == null || clientCast.IsDisposed)
            {
                return;
            }

            if (UnitViewAnimatorHelper.Uses3DAnimator(unit))
            {
                if (!clientCastComponent.HasOtherCast(args.CastId))
                {
                    UnitViewAnimatorHelper.TryPlayMotion(unit, MotionType.Idle, 1f);
                }

                await ETTask.CompletedTask;
                return;
            }

            // 打断后强制回 Idle；此时尚未 Remove ClientCast，Play() 会被 IsCasting 拦截。
            Animator2DComponent animator = unit.GetComponent<Animator2DComponent>();
            if (animator?.AnimPlayer != null)
            {
                animator.SyncFacingFromUnit();
                animator.AnimPlayer.Play((int)MotionType.Idle, animator.Facing, 1f);
            }

            await ETTask.CompletedTask;
        }
    }
}