using Unity.Mathematics;

namespace ET.Server
{
    [Actions(ActionsType.MoveToTarget)]
    [FriendOf(typeof(Actions))]
    [FriendOf(typeof(Cast))]
    [FriendOf(typeof(BulletComponent))]
    public class Actions_MoveToTarget : IActions
    {
        public void Run(Actions actions, ActionsRunType actionsRunType)
        {
            Unit unit = GetMoveUnit(actions, actionsRunType);
            if (unit == null || unit.IsDisposed)
            {
                return;
            }

            ActionsConfig config = actions.Config;
            if (config.ActionsParam == null || config.ActionsParam.Length < 1)
            {
                Log.Error($"Actions_MoveToTarget ActionsParam invalid: configId={config.Id}");
                return;
            }

            float moveStep = config.ActionsParam[0] / 1000f;
            float moveStepSq = moveStep * moveStep;

            using (ListComponent<Unit> targets = ListComponent<Unit>.Create())
            {
                actions.CollectActionTargets(actionsRunType, targets);
                if (targets.Count > 0)
                {
                    ApplyMoveToTarget(unit, targets[0], moveStep, moveStepSq);
                }
                else
                {
                    ApplyMoveForward(unit, moveStep);
                }
            }
        }

        private static Unit GetMoveUnit(Actions actions, ActionsRunType actionsRunType)
        {
            switch (actionsRunType)
            {
                case ActionsRunType.CastHit:
                    return actions.CastSelf?.Caster;
                case ActionsRunType.BulletTick:
                    return actions.BulletSelf?.GetParent<Unit>();
                default:
                    return null;
            }
        }

        private static void ApplyMoveToTarget(Unit unit, Unit tar, float moveStep, float moveStepSq)
        {
            float3 offset = tar.Position - unit.Position;
            float distSq = math.lengthsq(offset);
            if (distSq <= 0.001f)
            {
                return;
            }

            float3 newPos;
            if (distSq <= moveStepSq)
            {
                newPos = tar.Position;
                newPos.y = unit.Position.y;
                unit.ForceSetPosition(newPos, true);
                Log.Console($"unit {unit.Id} 向目标移动到达 newPos:{newPos}");
                return;
            }

            newPos = unit.Position + math.normalize(offset) * moveStep;
            newPos.y = unit.Position.y;
            unit.ForceSetPosition(newPos, true);
            Log.Console($"unit {unit.Id} 向目标移动{moveStep}米 newPos:{newPos}");
        }

        private static void ApplyMoveForward(Unit unit, float moveStep)
        {
            float3 newPos = unit.Position + math.normalize(unit.Forward) * moveStep;
            newPos.y = unit.Position.y;
            unit.ForceSetPosition(newPos, true);
            Log.Console($"unit {unit.Id} 向目标移动{moveStep}米 newPos:{newPos}");
        }
    }
}
