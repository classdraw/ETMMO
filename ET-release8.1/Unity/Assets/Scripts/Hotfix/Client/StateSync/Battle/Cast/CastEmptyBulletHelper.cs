namespace ET.Client
{
    [FriendOf(typeof(FollowComponent))]
    public static class CastEmptyBulletHelper
    {
        public static bool TryParseAction(int actionId, out int effectConfigId, out int flyTimeMs)
        {
            effectConfigId = 0;
            flyTimeMs = 0;

            if (!ActionsConfigCategory.Instance.Contain(actionId))
            {
                Log.Error($"CastEmptyBulletHelper ActionsConfig not found: {actionId}");
                return false;
            }

            ActionsConfig actionsConfig = ActionsConfigCategory.Instance.Get(actionId);
            if (actionsConfig.ActionsParam == null || actionsConfig.ActionsParam.Length < 2)
            {
                Log.Error($"CastEmptyBulletHelper ActionsParam invalid, actionId={actionId}");
                return false;
            }

            effectConfigId = actionsConfig.ActionsParam[0];
            flyTimeMs = actionsConfig.ActionsParam[1];
            if (flyTimeMs <= 0)
            {
                Log.Error($"CastEmptyBulletHelper flyTimeMs invalid, actionId={actionId}, flyTimeMs={flyTimeMs}");
                return false;
            }

            if (!EffectConfigCategory.Instance.Contain(effectConfigId))
            {
                Log.Error($"CastEmptyBulletHelper EffectConfig not found: {effectConfigId}");
                return false;
            }

            return true;
        }

        public static Unit CreateBullet(Scene scene, Unit caster, Unit target, int flyTimeMs)
        {
            Unit bulletUnit = UnitFactory.CreateEmptyBullet(scene, caster);
            FollowComponent followComponent = bulletUnit.GetComponent<FollowComponent>();
            followComponent.Target = target;
            followComponent.FlyTimeMs = flyTimeMs;
            followComponent.IsReady = false;
            return bulletUnit;
        }
    }
}
