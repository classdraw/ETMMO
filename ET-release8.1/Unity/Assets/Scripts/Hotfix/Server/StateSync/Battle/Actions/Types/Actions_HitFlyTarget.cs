using Unity.Mathematics;

namespace ET.Server
{
    [Actions(ActionsType.HitFlyTarget)]
    [FriendOf(typeof(Actions))]
    [FriendOf(typeof(Cast))]
    [FriendOf(typeof(Buff))]
    public class Actions_HitFlyTarget : IActions
    {
        public void Run(Actions actions, ActionsRunType actionsRunType)
        {
            if (actionsRunType != ActionsRunType.CastHit && actionsRunType != ActionsRunType.BuffTick)
            {
                return;
            }

            if (!TryGetBuffContext(actions, actionsRunType, out long addUnitId, out int addCastId))
            {
                return;
            }

            Unit caster = actionsRunType == ActionsRunType.CastHit ? actions.Caster : actions.Owner;
            if (caster == null || caster.IsDisposed || !caster.IsBattleUnit())
            {
                return;
            }

            ActionsConfig config = actions.Config;
            if (config.ActionsParam == null || config.ActionsParam.Length < 2)
            {
                Log.Error($"Actions_HitFlyTarget ActionsParam invalid: configId={config.Id}");
                return;
            }

            float dir = config.ActionsParam[1] / 1000f;
            int buffId = config.ActionsParam.Length > 2 ? config.ActionsParam[2] : 0;

            float3 forwardDir = caster.Forward;
            forwardDir.y = 0;
            if (math.lengthsq(forwardDir) <= 0.001f)
            {
                return;
            }

            forwardDir = math.normalize(forwardDir);

            actions.ForEachActionTarget(actionsRunType,
                target => ApplyHitFly(target, forwardDir, dir, buffId, addUnitId, addCastId));
        }

        private static bool TryGetBuffContext(Actions actions, ActionsRunType actionsRunType, out long addUnitId, out int addCastId)
        {
            addUnitId = 0;
            addCastId = 0;

            if (actionsRunType == ActionsRunType.BuffTick)
            {
                Buff buff = actions.BuffSelf;
                if (buff == null)
                {
                    return false;
                }

                addUnitId = buff.AddUnitId;
                addCastId = buff.AddSkillId;
                return true;
            }

            Cast cast = actions.CastSelf;
            if (cast == null)
            {
                return false;
            }

            Unit caster = cast.Caster;
            if (caster != null && !caster.IsDisposed)
            {
                addUnitId = caster.Id;
            }

            addCastId = cast.ConfigId;
            return true;
        }

        private static void ApplyHitFly(Unit unit, float3 forwardDir, float dir, int buffId, long addUnitId, int addCastId)
        {
            float3 unitPos = unit.Position;
            float3 newPos = unitPos + forwardDir * dir;
            newPos.y = unitPos.y;
            unit.FindPathMoveToAsync(newPos).Coroutine();

            if (buffId != 0)
            {
                unit.GetComponent<BuffComponent>()?.CreateAndAdd(buffId, addUnitId, addCastId);
            }

            Log.Console($"击飞目标 {unit.Id} 新位置: {newPos}");
        }
    }
}
