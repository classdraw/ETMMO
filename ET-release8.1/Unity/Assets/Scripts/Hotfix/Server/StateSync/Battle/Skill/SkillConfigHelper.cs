namespace ET.Server
{
    /// <summary>SkillConfig 与 CastConfig 映射。</summary>
    public static class SkillConfigHelper
    {
        public static bool TryGetCastConfigId(int skillConfigId, out int castConfigId)
        {
            castConfigId = 0;
            if (!SkillConfigCategory.Instance.Contain(skillConfigId))
            {
                return false;
            }

            castConfigId = SkillConfigCategory.Instance.Get(skillConfigId).CastId;
            return CastConfigCategory.Instance.Contain(castConfigId);
        }
    }
}