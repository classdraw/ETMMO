namespace ET.Server
{
    /// <summary>
    /// 按 UnBreakTime 区分行为技能与附属技能。
    /// 行为技能（UnBreakTime>=0）占用 CurrentSkill 并参与打断；附属技能（UnBreakTime==-1）仅走 Cast 实例与 CD。
    /// </summary>
    public static class CastSkillKind
    {
        public static bool IsBehaviorSkill(CastConfig config)
        {
            return config != null && config.UnBreakTime >= 0;
        }

        public static bool IsAttachedSkill(CastConfig config)
        {
            return config != null && config.UnBreakTime == -1;
        }

        public static bool IsBehaviorSkill(Cast cast)
        {
            return cast != null && !cast.IsDisposed && IsBehaviorSkill(cast.Config);
        }

        public static bool IsAttachedSkill(Cast cast)
        {
            return cast != null && !cast.IsDisposed && IsAttachedSkill(cast.Config);
        }
    }
}
