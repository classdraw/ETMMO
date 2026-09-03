namespace ET.Client
{
    [Event(SceneType.Current)]
    [FriendOf(typeof(Animator2DComponent))]
    public class ChangeRotation_SyncAnimator2DFacing : AEvent<Scene, ChangeRotation>
    {
        protected override async ETTask Run(Scene scene, ChangeRotation args)
        {
            Unit unit = args.Unit;
            if (unit == null || unit.IsDisposed)
            {
                return;
            }

            Animator2DComponent animator = unit.GetComponent<Animator2DComponent>();
            if (animator == null || animator.IsDisposed)
            {
                return;
            }

            animator.SyncFacingFromUnit();
            await ETTask.CompletedTask;
        }
    }
}
