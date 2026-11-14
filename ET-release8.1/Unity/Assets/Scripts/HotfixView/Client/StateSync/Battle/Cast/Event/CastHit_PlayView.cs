using System;

namespace ET.Client
{
    //技能命中逻辑
    [Event(SceneType.Current)]
    [FriendOf(typeof(ClientCastComponent))]
    [FriendOf(typeof(ClientCast))]
    [FriendOfAttribute(typeof(ET.Client.Animator2DComponent))]
    public class CastHit_PlayView : AEvent<Scene, CastHit>
    {
        protected override async ETTask Run(Scene scene, CastHit args)
        {
            UnitComponent unitComponent = scene.GetComponent<UnitComponent>();
            Unit caster = unitComponent.Get(args.CasterId);
            if (caster == null || caster.IsDisposed)
            {
                return;
            }

            ClientCastComponent clientCastComponent = caster.GetComponent<ClientCastComponent>();
            if (clientCastComponent == null || clientCastComponent.IsDisposed)
            {
                return;
            }

            ClientCast clientCast = clientCastComponent.Get(args.CastId);
            if (clientCast == null || clientCast.IsDisposed)
            {
                return;
            }

            if (args.IsSelf)
            {
                PlayHitAnimation(clientCast.Config, true, caster);
                await PlayHitEffect(clientCast.Config, true, args.HitIndex, caster);
            }
            else
            {
                Unit target = unitComponent.Get(args.TargetId);
                if (target == null || target.IsDisposed)
                {
                    return;
                }

                PlayHitAnimation(clientCast.Config, false, target);
                await PlayHitEffect(clientCast.Config, false, args.HitIndex, target);
            }
        }

        private static void PlayHitAnimation(CastConfig castConfig, bool isSelf, Unit unit)
        {
            int animation = isSelf ? castConfig.SelfHitAnimation : castConfig.HitAnimation;
            bool useDefaultHit = animation <= 0;
            MotionType motionType;
            if (useDefaultHit)
            {
                // 自己受击仍看配置；目标没配时默认 Hit，怪物控制器里有这个状态。
                if (isSelf)
                {
                    return;
                }

                motionType = MotionType.Hit;
            }
            else if (!Enum.IsDefined(typeof(MotionType), animation))
            {
                Log.Error($"CastHit_PlayView invalid hit animation: {animation}, castConfigId={castConfig.Id}");
                return;
            }
            else
            {
                motionType = (MotionType)animation;
                if (motionType == MotionType.None)
                {
                    return;
                }
            }

            if (UnitViewAnimatorHelper.Uses3DAnimator(unit))
            {
                AnimatorComponent animator3D = unit.GetComponent<AnimatorComponent>();
                if (animator3D == null || !animator3D.TryGetClipLength(motionType, out _))
                {
                    if (!useDefaultHit)
                    {
                        Log.Warning($"CastHit_PlayView 找不到受击动作 {motionType}, unitId={unit.Id}, castConfigId={castConfig.Id}");
                    }

                    return;
                }

                UnitViewAnimatorHelper.TryPlayMotionInTime(unit, motionType, UnitViewAnimatorHelper.HitPlayDurationMs);
                return;
            }

            Animator2DComponent animator = unit.GetComponent<Animator2DComponent>();
            if (animator == null || animator.IsDisposed || animator.AnimPlayer == null)
            {
                return;
            }

            if (animator.AnimationTime(motionType) <= 0f)
            {
                if (!useDefaultHit)
                {
                    Log.Warning($"CastHit_PlayView 找不到2D受击动作 {motionType}, unitId={unit.Id}, castConfigId={castConfig.Id}");
                }

                return;
            }

            animator.PlayInTime(motionType, UnitViewAnimatorHelper.HitPlayDurationMs / 1000f);
        }

        private static async ETTask PlayHitEffect(CastConfig castConfig, bool isSelf, int hitIndex, Unit effectUnit)
        {
            int[] effectIds = isSelf ? castConfig.SelfHitEffect : castConfig.HitEffect;
            if (effectIds == null || hitIndex < 0 || hitIndex >= effectIds.Length)
            {
                return;
            }

            int effectConfigId = effectIds[hitIndex];
            if (effectConfigId == 0)
            {
                return;
            }

            MountComponent mountComponent = effectUnit.GetComponent<MountComponent>();
            if (mountComponent == null || mountComponent.IsDisposed)
            {
                return;
            }

            await mountComponent.MountEffect(effectConfigId);
        }
    }
}
