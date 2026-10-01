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
            if (actionsRunType != ActionsRunType.CastHit
                && actionsRunType != ActionsRunType.BuffTick
                && actionsRunType != ActionsRunType.BulletTick)
            {
                return;
            }

            Unit unit = GetMoveUnit(actions, actionsRunType);
            if (unit == null || unit.IsDisposed || !unit.IsBattleUnit())
            {
                return;
            }

            ActionsConfig config = actions.Config;
            if (config.ActionsParam == null || config.ActionsParam.Length < 2)
            {
                Log.Error($"Actions_MoveToTarget ActionsParam invalid: configId={config.Id}");
                return;
            }

            MoveToTargetMode moveMode = (MoveToTargetMode)config.ActionsParam[0];
            float moveStep = config.ActionsParam[1] / 1000f;
            float moveStepSq = moveStep * moveStep;
            bool snapNavMesh = config.ActionsParam.Length >= 3
                && (MoveToTargetNavSnap)config.ActionsParam[2] == MoveToTargetNavSnap.SnapNavMesh;

            if (!TryComputeMoveDestination(actions, actionsRunType, unit, moveMode, moveStep, moveStepSq, out float3 destination))
            {
                return;
            }

            MoveUnitStep(unit, destination, snapNavMesh);
        }

        private static void MoveUnitStep(Unit unit, float3 destination, bool snapNavMesh)
        {
            unit.ActionDisplacementMove(destination, snapNavMesh);
        }

        private static bool TryComputeMoveDestination(Actions actions, ActionsRunType actionsRunType, Unit unit,
        MoveToTargetMode moveMode, float moveStep, float moveStepSq, out float3 destination)
        {
            destination = default;
            switch (moveMode)
            {
                case MoveToTargetMode.Input:
                    return TryComputeInputDestination(actions, actionsRunType, unit, moveStep, moveStepSq, out destination);
                case MoveToTargetMode.Forward:
                    return TryComputeForwardDestination(unit, moveStep, out destination);
                case MoveToTargetMode.Target:
                    return TryComputeTargetDestination(actions, actionsRunType, unit, moveStep, moveStepSq, out destination);
                default:
                    return TryComputeForwardDestination(unit, moveStep, out destination);
            }
        }

        private static bool TryComputeInputDestination(Actions actions, ActionsRunType actionsRunType, Unit unit, float moveStep,
        float moveStepSq, out float3 destination)
        {
            if (!TryGetInput(actions, actionsRunType, out long inputUnitId, out float3 inputPos))
            {
                return TryComputeForwardDestination(unit, moveStep, out destination);
            }

            Unit inputUnit = GetInputUnit(actions, inputUnitId);
            if (inputUnit != null)
            {
                return TryComputeStepToward(unit, inputUnit.Position, moveStep, moveStepSq, out destination);
            }

            return TryComputeStepToward(unit, inputPos, moveStep, moveStepSq, out destination);
        }

        private static bool TryComputeTargetDestination(Actions actions, ActionsRunType actionsRunType, Unit unit, float moveStep,
        float moveStepSq, out float3 destination)
        {
            using (ListComponent<Unit> targets = ListComponent<Unit>.Create())
            {
                actions.CollectActionTargets(actionsRunType, targets);
                if (targets.Count <= 0)
                {
                    return TryComputeForwardDestination(unit, moveStep, out destination);
                }

                return TryComputeStepToward(unit, targets[0].Position, moveStep, moveStepSq, out destination);
            }
        }

        private static bool TryComputeForwardDestination(Unit unit, float moveStep, out float3 destination)
        {
            destination = default;
            float3 forward = unit.Forward;
            forward.y = 0;
            if (math.lengthsq(forward) <= 0.001f)
            {
                return false;
            }

            destination = unit.Position + math.normalize(forward) * moveStep;
            destination.y = unit.Position.y;
            return true;
        }

        private static bool TryComputeStepToward(Unit unit, float3 targetPos, float moveStep, float moveStepSq, out float3 destination)
        {
            destination = default;
            float3 offset = targetPos - unit.Position;
            offset.y = 0;
            float distSq = math.lengthsq(offset);
            if (distSq <= 0.001f)
            {
                return false;
            }

            if (distSq <= moveStepSq)
            {
                destination = targetPos;
                destination.y = unit.Position.y;
                return true;
            }

            destination = unit.Position + math.normalize(offset) * moveStep;
            destination.y = unit.Position.y;
            return true;
        }

        private static Unit GetMoveUnit(Actions actions, ActionsRunType actionsRunType)
        {
            switch (actionsRunType)
            {
                case ActionsRunType.CastHit:
                    return actions.CastSelf?.Caster;
                case ActionsRunType.BuffTick:
                    return actions.Owner;
                case ActionsRunType.BulletTick:
                    return actions.BulletSelf?.GetParent<Unit>();
                default:
                    return null;
            }
        }

        private static bool TryGetInput(Actions actions, ActionsRunType actionsRunType, out long inputUnitId, out float3 inputPos)
        {
            inputUnitId = 0;
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

                    inputUnitId = cast.InputUnitId;
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

                    inputUnitId = bullet.InputUnitId;
                    inputPos = bullet.InputPos;
                    return true;
                }
                default:
                    return false;
            }
        }

        private static Unit GetInputUnit(Actions actions, long inputUnitId)
        {
            if (inputUnitId == 0)
            {
                return null;
            }

            Unit inputUnit = actions.Scene()?.GetComponent<UnitComponent>()?.Get(inputUnitId);
            if (inputUnit == null || inputUnit.IsDisposed || !inputUnit.IsBattleUnit())
            {
                return null;
            }

            return inputUnit;
        }
    }
}
