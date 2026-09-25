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

            Unit center = actions.Caster;
            if (center == null || center.IsDisposed)
            {
                return;
            }

            float moveStep = config.ActionsParam[0] / 1000f;
            float moveStepSq = moveStep * moveStep;
            float moveStepIgnoreSq = config.ActionsParam[1] / 1000f;
            using (ListComponent<Unit> targets = ListComponent<Unit>.Create())
            {
                actions.CollectActionTargets(actionsRunType, targets);
                foreach (Unit target in targets)
                {
                    AttractUnitToward(target, center, moveStep, moveStepSq, moveStepIgnoreSq);
                }
            }
        }

        private static void AttractUnitToward(Unit u, Unit center, float moveStep, float moveStepSq, float moveStepIgnoreSq)
        {
            if (u == null || u.IsDisposed || !u.IsBattleUnit())
            {
                return;
            }

            float3 offset = center.Position - u.Position;
            float distSq = math.lengthsq(offset);
            if (distSq <= moveStepIgnoreSq)
            {
                return;
            }

            float3 newPos;
            if (distSq <= moveStepSq)
            {
                newPos = center.Position;
            }
            else
            {
                newPos = u.Position + math.normalize(offset) * moveStep;
            }

            newPos.y = u.Position.y;
            u.ForceSetPosition(newPos, true);
            Log.Console($"吸引目标 {u.Id} 新位置: {newPos}");
        }
    }
}
