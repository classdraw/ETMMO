namespace ET.Client
{
    [MessageHandler(SceneType.StateSync)]
    public class M2C_CastEmptyBulletHandler: MessageHandler<Scene, M2C_CastEmptyBullet>
    {
        protected override async ETTask Run(Scene root, M2C_CastEmptyBullet message)
        {
            Scene currentScene = root.CurrentScene();
            UnitComponent unitComponent = currentScene?.GetComponent<UnitComponent>();
            if (unitComponent == null)
            {
                return;
            }

            Unit caster = unitComponent.Get(message.CasterId);
            Unit target = unitComponent.Get(message.TargetId);
            if (caster == null || caster.IsDisposed || target == null || target.IsDisposed)
            {
                return;
            }

            await CastEmptyBulletHelper.Create(currentScene, caster, target, message.ActionId);
        }
    }
}
