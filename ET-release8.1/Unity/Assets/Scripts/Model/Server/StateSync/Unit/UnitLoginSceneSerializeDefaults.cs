namespace ET.Server
{
    /// <summary>
    /// 登录进场景时，对带 <see cref="IUnitCache"/> 的组件中不应持久化的运行时字段恢复默认值。
    /// <para>Final 禁止类：见 <see cref="TransientNumerics"/>。</para>
    /// <para>各属性 Temp 分量（Final×100+6~+9）：见 <see cref="ResetTransientNumeric"/> 内 <see cref="NumericComponentSystem.ResetAllTempComponents"/>。</para>
    /// </summary>
    [FriendOfAttribute(typeof(ET.Server.SkillStatusComponent))]    
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

        /// <summary>
        /// 清空 <see cref="SkillStatusComponent"/> 的运行时状态（CD、当前施法）；技能等级保留。
        /// </summary>
        public static bool ResetTransientSkillStatus(SkillStatusComponent skillStatus)
        {
            if (skillStatus == null)
            {
                return false;
            }

            bool changed = skillStatus.CurrentSkillCastInstanceId != default
                || skillStatus.CurrentSkillCastID != default
                || skillStatus.CurrentSkillStartTime != default
                || skillStatus.CurrentSkillStatus != SkillStatusType.New
                || skillStatus.CoolDowns.Count > 0
                || skillStatus.CoolDownStartTimes.Count > 0;

            skillStatus.CurrentSkillCastInstanceId = default;
            skillStatus.CurrentSkillCastID = default;
            skillStatus.CurrentSkillStartTime = default;
            skillStatus.CurrentSkillStatus = SkillStatusType.New;
            skillStatus.CoolDowns.Clear();
            skillStatus.CoolDownStartTimes.Clear();
            return changed;
        }
    }
}
