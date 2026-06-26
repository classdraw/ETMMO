using System.Collections.Generic;
using Unity.Mathematics;

namespace ET
{
    public static class ShapeSelectHelper
    {
        private const float MinDirectionSqr = 0.01f;

        public static bool IsMatchSelectCampType(Unit sourceUnit, Unit targetUnit, SelectCampType selectCampType)
        {
            return selectCampType switch
            {
                SelectCampType.Ally => CampHelper.IsAlly(sourceUnit, targetUnit),
                SelectCampType.Hostile => CampHelper.IsHostile(sourceUnit, targetUnit),
                _ => false,
            };
        }

        public static bool IsTargetValid(Unit sourceUnit, Unit targetUnit)
        {
            return targetUnit != sourceUnit && targetUnit.IsBattleUnit();
        }

        public static bool IsInCircle(float3 center, float3 targetPos, int radiusMm)
        {
            float radius = radiusMm / 1000f;
            float radiusSqr = radius * radius;
            return math.lengthsq(center - targetPos) <= radiusSqr;
        }

        public static bool TrySelectSingle(Unit unit, float3 pos, int radiusMm)
        {
            if (unit == null || !unit.IsBattleUnit())
            {
                return false;
            }

            return IsInCircle(pos, unit.Position, radiusMm);
        }

        public static int SelectCircle(Unit sourceUnit, float3 center, int radiusMm, int needCount, SelectCampType selectCampType,
            IEnumerable<Unit> candidates, ICollection<Unit> results)
        {
            int nowCount = 0;
            foreach (Unit targetUnit in candidates)
            {
                if (!IsTargetValid(sourceUnit, targetUnit))
                {
                    continue;
                }

                if (!IsMatchSelectCampType(sourceUnit, targetUnit, selectCampType))
                {
                    continue;
                }

                if (!IsInCircle(center, targetUnit.Position, radiusMm))
                {
                    continue;
                }

                results.Add(targetUnit);
                nowCount++;
                if (nowCount >= needCount)
                {
                    break;
                }
            }

            return nowCount;
        }

        public static int SelectRectangle(Unit sourceUnit, float3 center, int lengthMm, int heightMm, int directionType, int needCount,
            SelectCampType selectCampType, IEnumerable<Unit> candidates, ICollection<Unit> results)
        {
            float halfLength = lengthMm / 1000f * 0.5f;
            float halfHeight = heightMm / 1000f * 0.5f;
            bool axisAligned = directionType == 0;
            float3 forward = new float3(0, 0, 1);
            float3 right = new float3(1, 0, 0);
            if (directionType == 1)
            {
                float3 direction = center - sourceUnit.Position;
                direction.y = 0;
                if (math.lengthsq(direction) > MinDirectionSqr)
                {
                    forward = math.normalize(direction);
                    right = math.normalize(new float3(forward.z, 0, -forward.x));
                }
                else
                {
                    axisAligned = true;
                }
            }

            int nowCount = 0;
            foreach (Unit targetUnit in candidates)
            {
                if (!IsTargetValid(sourceUnit, targetUnit))
                {
                    continue;
                }

                if (!IsMatchSelectCampType(sourceUnit, targetUnit, selectCampType))
                {
                    continue;
                }

                if (!IsInRectangle(center, targetUnit.Position, halfLength, halfHeight, axisAligned, forward, right))
                {
                    continue;
                }

                results.Add(targetUnit);
                nowCount++;
                if (nowCount >= needCount)
                {
                    break;
                }
            }

            return nowCount;
        }

        public static bool IsInRectangle(float3 center, float3 targetPos, float halfLength, float halfHeight, bool axisAligned, float3 forward,
            float3 right)
        {
            float3 offset = targetPos - center;
            offset.y = 0;

            if (axisAligned)
            {
                return math.abs(offset.x) <= halfLength && math.abs(offset.z) <= halfHeight;
            }

            float localLength = math.dot(offset, forward);
            float localHeight = math.dot(offset, right);
            return math.abs(localLength) <= halfLength && math.abs(localHeight) <= halfHeight;
        }

        public static int SelectFan(Unit sourceUnit, float3 targetPos, int directionType, int angleMm, int needCount, SelectCampType selectCampType,
            IEnumerable<Unit> candidates, ICollection<Unit> results)
        {
            float halfAngle = angleMm / 1000f * 0.5f;
            float3 forward = GetFanForward(sourceUnit, targetPos, directionType);

            int nowCount = 0;
            foreach (Unit targetUnit in candidates)
            {
                if (!IsTargetValid(sourceUnit, targetUnit))
                {
                    continue;
                }

                if (!IsMatchSelectCampType(sourceUnit, targetUnit, selectCampType))
                {
                    continue;
                }

                if (!IsInFan(sourceUnit.Position, targetUnit.Position, forward, halfAngle))
                {
                    continue;
                }

                results.Add(targetUnit);
                nowCount++;
                if (nowCount >= needCount)
                {
                    break;
                }
            }

            return nowCount;
        }

        public static bool IsInFan(float3 origin, float3 targetPos, float3 forward, float halfAngle)
        {
            float3 toTarget = targetPos - origin;
            toTarget.y = 0;
            if (math.lengthsq(toTarget) <= MinDirectionSqr)
            {
                return false;
            }

            float3 toTargetDir = math.normalize(toTarget);
            float dot = math.clamp(math.dot(forward, toTargetDir), -1f, 1f);
            float angleToTarget = math.degrees(math.acos(dot));
            return angleToTarget <= halfAngle;
        }

        private static float3 GetFanForward(Unit sourceUnit, float3 targetPos, int directionType)
        {
            if (directionType == 1)
            {
                float3 direction = targetPos - sourceUnit.Position;
                direction.y = 0;
                if (math.lengthsq(direction) > MinDirectionSqr)
                {
                    return math.normalize(direction);
                }
            }

            float3 forward = sourceUnit.Forward;
            forward.y = 0;
            return math.lengthsq(forward) > MinDirectionSqr ? math.normalize(forward) : new float3(0, 0, 1);
        }
    }
}
