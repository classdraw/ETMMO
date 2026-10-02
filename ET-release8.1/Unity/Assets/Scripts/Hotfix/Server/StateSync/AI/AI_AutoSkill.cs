using Unity.Mathematics;

namespace ET.Server
{
    [FriendOf(typeof(AutoSkillComponent))]
    [FriendOf(typeof(XunLuoPathComponent))]
    public class AI_AutoSkill : AAIHandler
    {
        public override int Check(AIComponent aiComponent, AIConfig aiConfig)
        {
            Unit unit = aiComponent.GetParent<Unit>();
            if (unit == null)
            {
                return 1;
            }

            AutoSkillComponent autoSkill = unit.GetComponent<AutoSkillComponent>();
            if (autoSkill == null)
            {
                return 1;
            }

            if (aiConfig.NodeParams.Length == 0)
            {
                return 1;
            }

            if (TimeInfo.Instance.ServerNow() <= autoSkill.NextAttackTime)
            {
                return 1;
            }

            if (unit.IsCasting() || unit.IsForbidSkill())
            {
                return 1;
            }

            if (FindTarget(unit) == null)
            {
                return 1;
            }

            return 0;
        }

        public override int Update(AIComponent aiComponent, AIConfig aiConfig)
        {
            Unit unit = aiComponent.GetParent<Unit>();
            if (unit == null || aiConfig.NodeParams.Length == 0)
            {
                return 1;
            }

            AutoSkillComponent autoSkill = unit.GetComponent<AutoSkillComponent>();
            if (autoSkill == null)
            {
                return 1;
            }

            Unit target = FindTarget(unit);
            if (target == null)
            {
                return 1;
            }

            int castConfigId = RandomGenerator.RandomArray(aiConfig.NodeParams);

            XunLuoPathComponent path = unit.GetComponent<XunLuoPathComponent>();
            if (path != null)
            {
                path.NextMoveTime = TimeInfo.Instance.ServerNow() + RandomGenerator.RandomNumber(5 * 1000, 8 * 1000);
                path.PatrolPhase = 0;
            }

            unit.TryBreakCastingBeforeCast();
            unit.CreateAndCast(castConfigId, target.Id, target.Position, true);
            autoSkill.NextAttackTime = TimeInfo.Instance.ServerNow() + RandomGenerator.RandomNumber(5000, 10000);
            return 1;
        }

        private static bool IsHostileTarget(Unit self, Unit target)
        {
            if (target == null || target.IsDisposed || target == self)
            {
                return false;
            }

            if (!target.IsBattleSelect())
            {
                return false;
            }

            return CampHelper.IsHostile(self, target);
        }

        private static Unit FindTarget(Unit selfUnit, float attackRange = 8f)
        {
            float maxDistSqr = attackRange * attackRange;
            Unit nearest = null;
            float nearestDistSqr = maxDistSqr;

            AOIEntity selfAoi = selfUnit.GetComponent<AOIEntity>();
            if (selfAoi != null)
            {
                foreach (AOIEntity seen in selfAoi.GetSeeUnits().Values)
                {
                    Unit unit = seen?.Unit;
                    if (!IsHostileTarget(selfUnit, unit))
                    {
                        continue;
                    }

                    float distSqr = math.lengthsq(unit.Position - selfUnit.Position);
                    if (distSqr <= nearestDistSqr)
                    {
                        nearestDistSqr = distSqr;
                        nearest = unit;
                    }
                }

                if (nearest != null)
                {
                    return nearest;
                }
            }

            UnitComponent unitComponent = selfUnit.Scene()?.GetComponent<UnitComponent>();
            if (unitComponent == null)
            {
                return null;
            }

            foreach (Entity entity in unitComponent.Children.Values)
            {
                if (entity is not Unit unit)
                {
                    continue;
                }

                if (!IsHostileTarget(selfUnit, unit))
                {
                    continue;
                }

                float distSqr = math.lengthsq(unit.Position - selfUnit.Position);
                if (distSqr <= nearestDistSqr)
                {
                    nearestDistSqr = distSqr;
                    nearest = unit;
                }
            }

            return nearest;
        }
    }
}
