namespace ET
{
    /// <summary>
    /// 怪物专用 AI 组：AIConfig 表内组 Id（AIConfigId 字段）下所有节点 Name 均为 AI_Monster_*。
    /// 仅怪物单位可走；压测机器人等走旧节点（AI_XunLuo 等）。
    /// </summary>
    public static class MonsterAiConfigHelper
    {
        public const string MonsterNodeNamePrefix = "AI_Monster_";

        public static bool IsMonsterOnlyAiGroup(int aiGroupId)
        {
            if (aiGroupId <= 0)
            {
                return false;
            }

            if (!AIConfigCategory.Instance.AIConfigs.TryGetValue(aiGroupId, out var nodes) || nodes.Count == 0)
            {
                return false;
            }

            foreach (AIConfig node in nodes.Values)
            {
                if (!node.Name.StartsWith(MonsterNodeNamePrefix))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
