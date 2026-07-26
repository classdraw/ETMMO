using System;

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
            //播放动画
            PlayStartAnimation(unit,args);
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

        private static void PlayStartAnimation(Unit unit,CastStart args)
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

            AnimatorComponent animator = unit.GetComponent<AnimatorComponent>();
            if (animator == null || animator.IsDisposed)
            {
                return;
            }

            animator.Play(motionType, 1f);
        }
    }
}
