namespace ET.Server
{
    /// <summary>
    /// 登录进场景时，对 <see cref="NumericComponent"/>（IUnitCache 序列化）中不应持久化的字段恢复默认值。
    /// <para>Final 禁止类：见 <see cref="TransientNumerics"/>。</para>
    /// <para>各属性 Temp 分量（Final×100+6~+9）：见 <see cref="ResetTransientNumeric"/> 内 <see cref="NumericComponentSystem.ResetAllTempComponents"/>。</para>
    /// </summary>
    public static class UnitLoginSceneSerializeDefaults
    {
        [StaticField]
        public static readonly (int NumericType, long Value)[] TransientNumerics =
        {
            (NumericType.ForbidSkill, 0),
            (NumericType.ForbidMove, 0),
            (NumericType.ForbidRotation, 0),
        };

        /// <summary>
        /// 重置不应落库的运行时数值：Forbid* + 全部 TempAdd/TempPct/TempFinalAdd/TempFinalPct（+6~+9）。
        /// </summary>
        public static bool ResetTransientNumeric(NumericComponent numeric)
        {
            if (numeric == null)
            {
                return false;
            }

            bool changed = false;
            foreach ((int numericType, long value) in TransientNumerics)
            {
                if (numeric.GetByKey(numericType) == value)
                {
                    continue;
                }

                numeric.SetNoEvent(numericType, value);
                changed = true;
            }

            if (numeric.ResetAllTempComponents())
            {
                changed = true;
            }

            return changed;
        }
    }
}
