using ET;

namespace ET.Client
{
    [Event(SceneType.Current)]
    [FriendOf(typeof(AnimatorComponent))]
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

            AnimatorComponent animator = unit.GetComponent<AnimatorComponent>();
            if (animator == null)
            {
                await ETTask.CompletedTask;
                return;
            }
            
            animator.Play(MotionType.Run, 1f);
            await ETTask.CompletedTask;
        }
    }
}
