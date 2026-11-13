using Unity.Mathematics;

namespace ET.Server
{
    /// <summary>
    /// 怪物出生点巡逻。A～G 见 MonsterAiSetupHelper（D 警戒、E 追击范围脱战）。
    /// </summary>
    [FriendOf(typeof(MonsterPatrolComponent))]
    public class AI_Monster_XunLuo : AAIHandler
    {
        private const byte MoveIntentPatrol = 0;
        private const byte MoveIntentChase = 1;
        private const byte MoveIntentReturnSpawn = 2;

        public override int Check(AIComponent aiComponent, AIConfig aiConfig)
        {
            Unit unit = aiComponent.GetParent<Unit>();
            if (unit == null || !unit.IsMonster())
            {
                return 1;
            }

            MonsterPatrolComponent patrol = unit.GetComponent<MonsterPatrolComponent>();
            if (patrol == null || patrol.PatrolRadiusM <= 0f)
            {
                return 1;
            }

            if (unit.IsCasting() || unit.IsForbidMove())
            {
                return 1;
            }

            return 0;
        }

        public override int Update(AIComponent aiComponent, AIConfig aiConfig)
        {
            Unit unit = aiComponent.GetParent<Unit>();
            if (unit == null || !unit.IsMonster())
            {
                return 1;
            }

            MonsterPatrolComponent patrol = unit.GetComponent<MonsterPatrolComponent>();
            if (patrol == null || patrol.PatrolRadiusM <= 0f)
            {
                return 1;
            }

            MoveComponent move = unit.GetComponent<MoveComponent>();

            if (unit.IsCasting() || unit.IsForbidMove())
            {
                if (move != null && !move.IsArrived())
                {
                    move.Stop(false);
                }

                patrol.PatrolPhase = 0;
                return 0;
            }

            if (patrol.PatrolPhase == 1)
            {
                if (move != null && !move.IsArrived())
                {
                    if (patrol.MoveIntent == MoveIntentChase && ShouldAbortChase(unit, patrol))
                    {
                        move.Stop(false);
                        patrol.PatrolPhase = 0;
                        BeginReturnToSpawn(unit, patrol);
                    }

                    return 0;
                }

                OnMoveArrived(patrol);
                return 0;
            }

            if (patrol.PatrolPhase != 0)
            {
                return 0;
            }

            if (TimeInfo.Instance.ServerNow() < patrol.NextMoveTime)
            {
                return 0;
            }

            if (patrol.ChaseEnabled && TryBeginChase(unit, patrol))
            {
                return 0;
            }

            BeginRandomPatrol(unit, patrol);
            return 0;
        }

        private static void OnMoveArrived(MonsterPatrolComponent patrol)
        {
            patrol.PatrolPhase = 0;
            byte intent = patrol.MoveIntent;

            if (intent == MoveIntentReturnSpawn)
            {
                patrol.MoveIntent = MoveIntentPatrol;
                patrol.ChaseTargetId = 0;
                patrol.NextMoveTime = TimeInfo.Instance.ServerNow() + patrol.PatrolIntervalMs;
                return;
            }

            if (intent == MoveIntentChase)
            {
                patrol.NextMoveTime = TimeInfo.Instance.ServerNow() + patrol.ChaseRestMs;
                return;
            }

            patrol.MoveIntent = MoveIntentPatrol;
            patrol.NextMoveTime = TimeInfo.Instance.ServerNow() + patrol.PatrolIntervalMs;
        }

        private static void BeginRandomPatrol(Unit unit, MonsterPatrolComponent patrol)
        {
            float radiusM = patrol.PatrolRadiusM;
            float dx = RandomGenerator.RandomNumber((int)(-radiusM * 1000f), (int)(radiusM * 1000f)) / 1000f;
            float dz = RandomGenerator.RandomNumber((int)(-radiusM * 1000f), (int)(radiusM * 1000f)) / 1000f;
            float3 target = patrol.SpawnCenter + new float3(dx, 0f, dz);
            target.y = patrol.SpawnCenter.y;

            unit.FindPathMoveToAsync(target).Coroutine();
            patrol.MoveIntent = MoveIntentPatrol;
            patrol.PatrolPhase = 1;
        }

        private static bool TryBeginChase(Unit unit, MonsterPatrolComponent patrol)
        {
            Unit player = FindChasePlayer(unit, patrol);
            if (player == null)
            {
                patrol.ChaseTargetId = 0;
                return false;
            }

            if (!TryGetChaseMoveTarget(unit, player, patrol, out float3 target))
            {
                patrol.ChaseTargetId = player.Id;
                int waitMs = patrol.ChaseRestMs > 0 ? patrol.ChaseRestMs : 200;
                patrol.NextMoveTime = TimeInfo.Instance.ServerNow() + waitMs;
                return true;
            }

            unit.FindPathMoveToAsync(target).Coroutine();
            patrol.ChaseTargetId = player.Id;
            patrol.MoveIntent = MoveIntentChase;
            patrol.PatrolPhase = 1;
            return true;
        }

        private static void BeginReturnToSpawn(Unit unit, MonsterPatrolComponent patrol)
        {
            patrol.ChaseTargetId = 0;
            float3 target = patrol.SpawnCenter;
            target.y = unit.Position.y;
            unit.FindPathMoveToAsync(target).Coroutine();
            patrol.MoveIntent = MoveIntentReturnSpawn;
            patrol.PatrolPhase = 1;
        }

        private static bool ShouldAbortChase(Unit unit, MonsterPatrolComponent patrol)
        {
            Unit player = GetChaseTargetUnit(unit, patrol);
            if (player == null)
            {
                return true;
            }

            if (patrol.ChaseRangeM <= 0f)
            {
                return false;
            }

            float distMonster = HorizontalDistance(unit.Position, patrol.SpawnCenter);
            float distPlayer = HorizontalDistance(player.Position, patrol.SpawnCenter);
            return distMonster > patrol.ChaseRangeM || distPlayer > patrol.ChaseRangeM;
        }

        private static bool TryGetChaseMoveTarget(Unit unit, Unit player, MonsterPatrolComponent patrol, out float3 target)
        {
            target = player.Position;
            float3 offset = player.Position - unit.Position;
            offset.y = 0f;
            float dist = math.length(offset);
            if (dist <= patrol.ChaseMinDistanceM + 0.01f)
            {
                return false;
            }

            if (dist > 0.001f)
            {
                float3 dir = math.normalize(offset);
                target = player.Position - dir * patrol.ChaseMinDistanceM;
            }

            target.y = player.Position.y;
            return true;
        }

        private static Unit GetChaseTargetUnit(Unit unit, MonsterPatrolComponent patrol)
        {
            if (patrol.ChaseTargetId == 0)
            {
                return null;
            }

            Unit target = unit.Scene()?.GetComponent<UnitComponent>()?.Get(patrol.ChaseTargetId);
            if (!IsChaseablePlayer(unit, target))
            {
                return null;
            }

            return target;
        }

        private static Unit FindChasePlayer(Unit unit, MonsterPatrolComponent patrol)
        {
            Unit nearest = null;
            float nearestDistSqr = float.MaxValue;

            AOIEntity selfAoi = unit.GetComponent<AOIEntity>();
            if (selfAoi != null)
            {
                foreach (AOIEntity seen in selfAoi.GetSeeUnits().Values)
                {
                    Unit candidate = seen?.Unit;
                    if (!IsChaseablePlayer(unit, candidate))
                    {
                        continue;
                    }

                    if (!IsPlayerInChaseLeash(patrol, candidate.Position))
                    {
                        continue;
                    }

                    if (!IsPlayerInAlertRange(unit, candidate, patrol))
                    {
                        continue;
                    }

                    float distSqr = math.lengthsq(candidate.Position - unit.Position);
                    if (distSqr < nearestDistSqr)
                    {
                        nearestDistSqr = distSqr;
                        nearest = candidate;
                    }
                }

                if (nearest != null)
                {
                    return nearest;
                }
            }

            UnitComponent unitComponent = unit.Scene()?.GetComponent<UnitComponent>();
            if (unitComponent == null)
            {
                return null;
            }

            foreach (Entity entity in unitComponent.Children.Values)
            {
                if (entity is not Unit candidate)
                {
                    continue;
                }

                if (!IsChaseablePlayer(unit, candidate))
                {
                    continue;
                }

                if (!IsPlayerInChaseLeash(patrol, candidate.Position))
                {
                    continue;
                }

                if (!IsPlayerInAlertRange(unit, candidate, patrol))
                {
                    continue;
                }

                float distSqr = math.lengthsq(candidate.Position - unit.Position);
                if (distSqr < nearestDistSqr)
                {
                    nearestDistSqr = distSqr;
                    nearest = candidate;
                }
            }

            return nearest;
        }

        private static bool IsChaseablePlayer(Unit self, Unit target)
        {
            if (target == null || target.IsDisposed || target == self)
            {
                return false;
            }

            if (!target.IsPlayer())
            {
                return false;
            }

            if (!target.IsBattleSelect())
            {
                return false;
            }

            return CampHelper.IsHostile(self, target);
        }

        private static float HorizontalDistance(float3 a, float3 b)
        {
            float3 d = a - b;
            d.y = 0f;
            return math.length(d);
        }

        /// <summary>玩家相对出生点仍在追击（脱战）范围内，才允许选中或继续追。</summary>
        private static bool IsPlayerInChaseLeash(MonsterPatrolComponent patrol, float3 playerPos)
        {
            if (patrol.ChaseRangeM <= 0f)
            {
                return true;
            }

            return HorizontalDistance(playerPos, patrol.SpawnCenter) <= patrol.ChaseRangeM;
        }

        /// <summary>玩家进入以怪物为圆心的警戒范围才开追。</summary>
        private static bool IsPlayerInAlertRange(Unit unit, Unit player, MonsterPatrolComponent patrol)
        {
            if (patrol.AlertRangeM <= 0f)
            {
                return true;
            }

            return HorizontalDistance(unit.Position, player.Position) <= patrol.AlertRangeM;
        }
    }
}
