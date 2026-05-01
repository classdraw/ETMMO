using ET;

namespace ET.Client
{
    [Event(SceneType.Current)]
    public class MoveStop_UnitAnimatorPlay : AEvent<Scene, MoveStop>
    {
        protected override async ETTask Run(Scene scene, MoveStop args)
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

            animator.Play(MotionType.Idle, 1f);
            await ETTask.CompletedTask;
        }
    }
}
