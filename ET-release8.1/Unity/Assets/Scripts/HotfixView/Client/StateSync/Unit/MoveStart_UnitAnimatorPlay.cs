using ET;

namespace ET.Client
{
    [Event(SceneType.Current)]
    [FriendOf(typeof(Animator2DComponent))]
    public class MoveStart_UnitAnimatorPlay : AEvent<Scene, MoveStart>
    {
        protected override async ETTask Run(Scene scene, MoveStart args)
        {
            Unit unit = args.Unit;
            if (unit == null || unit.IsDisposed)
            {
                await ETTask.CompletedTask;
                return;
            }

            Animator2DComponent animator = unit.GetComponent<Animator2DComponent>();
            if (animator == null)
            {
                await ETTask.CompletedTask;
                return;
            }

            animator.SyncFacingFromUnit();
            animator.Play(MotionType.Run, 1f);
            await ETTask.CompletedTask;
        }
    }
}
