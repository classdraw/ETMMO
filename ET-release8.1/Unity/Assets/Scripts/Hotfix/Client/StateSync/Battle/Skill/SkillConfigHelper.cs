namespace ET.Client
{
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