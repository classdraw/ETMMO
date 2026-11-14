using System;

namespace ET.Client
{
    //技能开始释放逻辑
    [Event(SceneType.Current)]
    [FriendOfAttribute(typeof(ET.Client.Animator2DComponent))]
    public class CastStart_PlayView : AEvent<Scene, CastStart>
    {
        protected override async ETTask Run(Scene scene, CastStart args)
        {
            Unit unit = scene.GetComponent<UnitComponent>().Get(args.CasterId);
            if (unit == null || unit.IsDisposed)
            {
                return;
            }
            //播放动画
            PlayStartAnimation(unit, args);
            await PlayStartEffect(unit, args.CasterConfigId);

            await ETTask.CompletedTask;
        }

        private static async ETTask PlayStartEffect(Unit unit, int castConfigId)
        {
            if (!CastConfigCategory.Instance.Contain(castConfigId))
            {
                Log.Error($"CastStart_PlayView CastConfig not found: {castConfigId}");
                return;
            }

            CastConfig castConfig = CastConfigCategory.Instance.Get(castConfigId);
            int[] startEffects = castConfig.StartEffect;
            if (startEffects == null || startEffects.Length == 0)
            {
                return;
            }

            MountComponent mountComponent = unit.GetComponent<MountComponent>();
            if (mountComponent == null || mountComponent.IsDisposed)
            {
                return;
            }

            foreach (int effectConfigId in startEffects)
            {
                if (effectConfigId == 0)
                {
                    continue;
                }

                await mountComponent.MountEffect(effectConfigId);
            }
        }

        private static void PlayStartAnimation(Unit unit, CastStart args)
        {
            if (!CastConfigCategory.Instance.Contain(args.CasterConfigId))
            {
                Log.Error($"CastStart_PlayView CastConfig not found: {args.CasterConfigId}");
                return;
            }

            CastConfig castConfig = CastConfigCategory.Instance.Get(args.CasterConfigId);
            if (castConfig.StartAnimation <= 0)
            {
                return;
            }

            if (!Enum.IsDefined(typeof(MotionType), castConfig.StartAnimation))
            {
                Log.Error($"CastStart_PlayView invalid StartAnimation: {castConfig.StartAnimation}, castConfigId={args.CasterConfigId}");
                return;
            }

            MotionType motionType = (MotionType)castConfig.StartAnimation;
            if (motionType == MotionType.None)
            {
                return;
            }

            if (UnitViewAnimatorHelper.Uses3DAnimator(unit))
            {
                UnitViewAnimatorHelper.TryPlayMotionInTime(unit, motionType, castConfig.TotalTime);
                return;
            }

            Animator2DComponent animator = unit.GetComponent<Animator2DComponent>();
            if (animator == null || animator.IsDisposed || animator.AnimPlayer == null)
            {
                return;
            }

            if (!animator.HasAnim(motionType))
            {
                Log.Warning($"CastStart_PlayView HasAnim failed, motionType={motionType}, castConfigId={args.CasterConfigId}, unitId={unit.Id}");
                return;
            }

            // 立即播放，避免 MoveStart/MoveStop 在同一帧覆盖 MotionType 队列。
            animator.SyncFacingFromUnit();
            float speed = 1f;
            float clipDuration = animator.AnimationTime(motionType);
            if (castConfig.TotalTime > 0 && clipDuration > 0f)
            {
                speed = clipDuration / (castConfig.TotalTime / 1000f);
                if (speed < 0.01f || speed > 1000f)
                {
                    Log.Error($"motionSpeed数值异常, {speed}, 动作 {motionType} 按原速播放");
                    speed = 1f;
                }
            }

            animator.AnimPlayer.Play((int)motionType, animator.Facing, speed);
        }
    }
}
