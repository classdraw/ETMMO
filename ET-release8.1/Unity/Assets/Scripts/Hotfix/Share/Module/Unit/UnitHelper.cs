namespace ET
{
    public static class UnitHelper
    {
        /// <summary>
        /// 是否处于禁止施法状态。NumericComponent 为空时不算禁止（由调用方单独处理组件缺失）。
        /// </summary>
        public static bool IsForbidSkill(this Unit unit)
        {
            if (unit == null || unit.IsDisposed)
            {
                return true;
            }

            NumericComponent numericComponent = unit.GetComponent<NumericComponent>();
            return numericComponent != null && numericComponent[NumericType.ForbidSkill] > 0;
        }

        /// <summary>
        /// 是否处于禁止移动状态。NumericComponent 为空时视为禁止移动。
        /// </summary>
        public static bool IsForbidMove(this Unit unit)
        {
            if (unit == null || unit.IsDisposed)
            {
                return true;
            }

            NumericComponent numericComponent = unit.GetComponent<NumericComponent>();
            return numericComponent == null || numericComponent[NumericType.ForbidMove] > 0;
        }

        /// <summary>
        /// 是否处于禁止转向状态。NumericComponent 为空时不算禁止。
        /// </summary>
        public static bool IsForbidRotation(this Unit unit)
        {
            if (unit == null || unit.IsDisposed)
            {
                return true;
            }

            NumericComponent numericComponent = unit.GetComponent<NumericComponent>();
            return numericComponent != null && numericComponent[NumericType.ForbidRotation] > 0;
        }
    }
}
