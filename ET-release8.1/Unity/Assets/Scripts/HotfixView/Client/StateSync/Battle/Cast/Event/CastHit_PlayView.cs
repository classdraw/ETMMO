namespace ET.Client
{
    //技能命中逻辑
    [Event(SceneType.Current)]
    [FriendOf(typeof(CastComponent))]
    [FriendOf(typeof(Cast))]
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

            CastComponent castComponent = caster.GetComponent<CastComponent>();
            if (castComponent == null || castComponent.IsDisposed)
            {
                return;
            }

            Cast cast = castComponent.Get(args.CastId);
            if (cast == null || cast.IsDisposed)
            {
                return;
            }

            int[] effectIds = args.IsSelf ? cast.Config.SelfHitEffect : cast.Config.HitEffect;
            if (effectIds == null || args.HitIndex < 0 || args.HitIndex >= effectIds.Length)
            {
                return;
            }

            int effectConfigId = effectIds[args.HitIndex];
            if (effectConfigId == 0)
            {
                return;
            }

            Unit effectUnit = args.IsSelf ? caster : unitComponent.Get(args.TargetId);
            if (effectUnit == null || effectUnit.IsDisposed)
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
