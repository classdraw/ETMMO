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
            bool snapNavMesh =  (MoveToTargetNavSnap)config.ActionsParam[2] == MoveToTargetNavSnap.SnapNavMesh;

            switch (moveMode)
            {
                case MoveToTargetMode.Input:
                    ApplyInputMove(actions, actionsRunType, unit, moveStep, moveStepSq, snapNavMesh);
                    break;
                case MoveToTargetMode.Forward:
                    ApplyMoveForward(unit, moveStep, snapNavMesh);
                    break;
                case MoveToTargetMode.Target:
                    ApplyTargetMove(actions, actionsRunType, unit, moveStep, moveStepSq, snapNavMesh);
                    break;
                default:
                    ApplyMoveForward(unit, moveStep, snapNavMesh);
                    break;
            }
        }

        private static void ApplyInputMove(Actions actions, ActionsRunType actionsRunType, Unit unit, float moveStep, float moveStepSq,
        bool snapNavMesh)
        {
            if (!TryGetInput(actions, actionsRunType, out long inputUnitId, out float3 inputPos))
            {
                ApplyMoveForward(unit, moveStep, snapNavMesh);
                return;
            }

            Unit inputUnit = GetInputUnit(actions, inputUnitId);
            if (inputUnit != null)
            {
                ApplyMoveToPos(unit, inputUnit.Position, moveStep, moveStepSq, snapNavMesh);
                return;
            }

            ApplyMoveToPos(unit, inputPos, moveStep, moveStepSq, snapNavMesh);
        }

        private static void ApplyTargetMove(Actions actions, ActionsRunType actionsRunType, Unit unit, float moveStep, float moveStepSq,
        bool snapNavMesh)
        {
            using (ListComponent<Unit> targets = ListComponent<Unit>.Create())
            {
                actions.CollectActionTargets(actionsRunType, targets);
                if (targets.Count <= 0)
                {
                    ApplyMoveForward(unit, moveStep, snapNavMesh);
                    return;
                }

                ApplyMoveToPos(unit, targets[0].Position, moveStep, moveStepSq, snapNavMesh);
            }
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

        private static void ApplyMoveToPos(Unit unit, float3 targetPos, float moveStep, float moveStepSq, bool snapNavMesh)
        {
            float3 offset = targetPos - unit.Position;
            offset.y = 0;
            float distSq = math.lengthsq(offset);
            if (distSq <= 0.001f)
            {
                return;
            }

            float3 newPos;
            if (distSq <= moveStepSq)
            {
                newPos = targetPos;
                newPos.y = unit.Position.y;
                unit.ActionStepSetPosition(newPos, snapNavMesh);
                return;
            }

            newPos = unit.Position + math.normalize(offset) * moveStep;
            newPos.y = unit.Position.y;
            unit.ActionStepSetPosition(newPos, snapNavMesh);
        }

        private static void ApplyMoveForward(Unit unit, float moveStep, bool snapNavMesh)
        {
            float3 forward = unit.Forward;
            forward.y = 0;
            if (math.lengthsq(forward) <= 0.001f)
            {
                return;
            }

            float3 newPos = unit.Position + math.normalize(forward) * moveStep;
            newPos.y = unit.Position.y;
            unit.ActionStepSetPosition(newPos, snapNavMesh);
        }
    }
}
