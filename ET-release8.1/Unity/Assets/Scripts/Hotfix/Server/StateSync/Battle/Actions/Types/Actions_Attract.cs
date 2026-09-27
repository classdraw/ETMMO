using Unity.Mathematics;

namespace ET.Server
{
    [Actions(ActionsType.Attract)]
    [FriendOf(typeof(Actions))]
    [FriendOf(typeof(Cast))]
    [FriendOf(typeof(BulletComponent))]
    public class Actions_Attract : IActions
    {
        public void Run(Actions actions, ActionsRunType actionsRunType)
        {
            if (actionsRunType != ActionsRunType.CastHit && actionsRunType != ActionsRunType.BulletTick)
            {
                return;
            }

            ActionsConfig config = actions.Config;
            if (config.ActionsParam == null || config.ActionsParam.Length <= 1)
            {
                Log.Error($"Actions_Attract ActionsParam invalid: configId={config.Id}");
                return;
            }

            Unit caster = actions.Caster;
            if (caster == null || caster.IsDisposed)
            {
                return;
            }

            float moveStep = config.ActionsParam[0] / 1000f;
            float moveStepSq = moveStep * moveStep;
            AttractCenterOrigin centerOrigin = (AttractCenterOrigin)config.ActionsParam[1];
            float moveStepIgnore = config.ActionsParam[2] / 1000f;
            float moveStepIgnoreSq = moveStepIgnore * moveStepIgnore;

            if (!TryResolveCenterPosition(actions, actionsRunType, caster, centerOrigin, out float3 centerPos))
            {
                return;
            }

            using (ListComponent<Unit> targets = ListComponent<Unit>.Create())
            {
                actions.CollectActionTargets(actionsRunType, targets);
                foreach (Unit target in targets)
                {
                    AttractUnitToward(target, centerPos, moveStep, moveStepSq, moveStepIgnoreSq);
                }
            }
        }

        private static bool TryResolveCenterPosition(Actions actions, ActionsRunType actionsRunType, Unit caster,
        AttractCenterOrigin centerOrigin, out float3 centerPos)
        {
            centerPos = caster.Position;
            if (centerOrigin == AttractCenterOrigin.Caster)
            {
                return true;
            }

            if (!TryGetInput(actions, actionsRunType, out float3 inputPos))
            {
                Log.Warning($"Actions_Attract centerOrigin=InputPos 但无输入: configId={actions.Config.Id}");
                return false;
            }

            if (math.lengthsq(inputPos) <= 0.001f)
            {
                Log.Warning($"Actions_Attract InputPos 无效，configId={actions.Config.Id}");
                return false;
            }

            centerPos = inputPos;
            return true;
        }

        private static bool TryGetInput(Actions actions, ActionsRunType actionsRunType, out float3 inputPos)
        {
            inputPos = default;
            switch (actionsRunType)
            {
                case ActionsRunType.CastHit:
                {
                    Cast cast = actions.CastSelf;
                    if (cast == null)
                    {
                        return false;
                    }

                    inputPos = cast.InputPos;
                    return true;
                }
                case ActionsRunType.BulletTick:
                {
                    BulletComponent bullet = actions.BulletSelf;
                    if (bullet == null)
                    {
                        return false;
                    }

                    inputPos = bullet.InputPos;
                    return true;
                }
                default:
                    return false;
            }
        }

        private static void AttractUnitToward(Unit u, float3 centerPos, float moveStep, float moveStepSq, float moveStepIgnoreSq)
        {
            if (u == null || u.IsDisposed || !u.IsBattleUnit())
            {
                return;
            }

            float3 offset = centerPos - u.Position;
            offset.y = 0;
            float distSq = math.lengthsq(offset);
            if (distSq <= moveStepIgnoreSq)
            {
                return;
            }

            float3 newPos;
            if (distSq <= moveStepSq)
            {
                newPos = centerPos;
            }
            else
            {
                newPos = u.Position + math.normalize(offset) * moveStep;
            }

            newPos.y = u.Position.y;
            u.ForceSetPosition(newPos, true);
        }
    }
}
