using System;

namespace ET.Client
{
    //技能命中逻辑
    [Event(SceneType.Current)]
    [FriendOf(typeof(ClientCastComponent))]
    [FriendOf(typeof(ClientCast))]
    public class CastHit_PlayView:AEvent<Scene,CastHit>
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
                PlayHitAnimation(caster, clientCast.Config, true);
                await PlayHitEffect(clientCast.Config, true, args.HitIndex, caster);

            }
            else
            {
                Unit target = unitComponent.Get(args.TargetId);
                if (target == null || target.IsDisposed)
                {
                    return;
                }

                PlayHitAnimation(target, clientCast.Config, false);
                await PlayHitEffect(clientCast.Config, false, args.HitIndex, target);
            }


        }

        private static void PlayHitAnimation(Unit unit, CastConfig castConfig, bool isSelf)
        {
            int animation = isSelf ? castConfig.SelfHitAnimation : castConfig.HitAnimation;
            if (animation <= 0)
            {
                return;
            }

            if (!Enum.IsDefined(typeof(MotionType), animation))
            {
                Log.Error($"CastHit_PlayView invalid hit animation: {animation}, isSelf={isSelf}, castConfigId={castConfig.Id}");
                return;
            }

            MotionType motionType = (MotionType)animation;
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
