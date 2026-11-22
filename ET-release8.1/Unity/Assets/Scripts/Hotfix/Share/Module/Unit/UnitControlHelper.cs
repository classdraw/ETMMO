namespace ET
{
    /// <summary>控制 / Debuff 数值（Buff +=1/-=1）与禁止施法、移动、转向判定。</summary>
    public static class UnitControlHelper
    {
        /// <summary>Buff Add +=；Buff Remove -=（不低于 0）</summary>
        public static void ApplyNumericRef(NumericComponent numeric, int numericType, int value, bool subtract)
        {
            if (numeric == null || value == 0)
            {
                return;
            }

            if (subtract)
            {
                long next = numeric[numericType] - value;
                numeric[numericType] = next < 0 ? 0 : next;
            }
            else
            {
                numeric[numericType] += value;
            }
        }

        public static bool IsForbidSkill(Unit unit)
        {
            if (unit == null || unit.IsDisposed)
            {
                return true;
            }

            NumericComponent numeric = unit.GetComponent<NumericComponent>();
            if (numeric == null)
            {
                return false;
            }

            return numeric[NumericType.ForbidSkill] > 0
                   || numeric[NumericType.Dizziness] > 0
                   || numeric[NumericType.Frozen] > 0;
        }

        public static bool IsForbidMove(Unit unit)
        {
            if (unit == null || unit.IsDisposed)
            {
                return true;
            }

            NumericComponent numeric = unit.GetComponent<NumericComponent>();
            if (numeric == null)
            {
                return true;
            }

            return numeric[NumericType.ForbidMove] > 0
                   || numeric[NumericType.Dizziness] > 0
                   || numeric[NumericType.Frozen] > 0;
        }

        public static bool IsForbidRotation(Unit unit)
        {
            if (unit == null || unit.IsDisposed)
            {
                return true;
            }

            NumericComponent numeric = unit.GetComponent<NumericComponent>();
            if (numeric == null)
            {
                return false;
            }

            return numeric[NumericType.ForbidRotation] > 0
                   || numeric[NumericType.Dizziness] > 0
                   || numeric[NumericType.Frozen] > 0;
        }
    }
}