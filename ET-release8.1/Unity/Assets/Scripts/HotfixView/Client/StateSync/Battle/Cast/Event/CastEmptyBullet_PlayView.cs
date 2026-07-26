namespace ET.Client
{
    [Event(SceneType.Current)]
    public class CastEmptyBullet_PlayView : AEvent<Scene, CastEmptyBullet>
    {
        protected override async ETTask Run(Scene scene, CastEmptyBullet args)
        {
            await CastEmptyBulletViewHelper.CreateView(scene, args.BulletUnit, args.Caster, args.Target, args.EffectConfigId, args.FlyTimeMs);
        }
    }
}
