using Unity.Mathematics;

namespace ET.Server
{
    /// <summary>怪物 AI：仅怪物单位 + 仅怪物专用 AIConfig 组（AI_Monster_* 节点）。</summary>
    [FriendOf(typeof(MonsterPatrolComponent))]
    public static class MonsterAiSetupHelper
    {
        public static void Setup(Unit unit, int aiConfigId, float3 spawnPos)
        {
            if (aiConfigId <= 0)
            {
                return;
            }

            if (!unit.IsMonster())
            {
                Log.Error($"[Monster] 非怪物单位不可挂怪物 AI: unit={unit.Id} type={unit.Type()} aiConfigId={aiConfigId}");
                return;
            }

            if (!MonsterAiConfigHelper.IsMonsterOnlyAiGroup(aiConfigId))
            {
                Log.Error($"[Monster] AIConfigId 不是怪物专用组（须全部为 {MonsterAiConfigHelper.MonsterNodeNamePrefix}* 节点）: aiConfigId={aiConfigId} unit={unit.Id}");
                return;
            }

            if (!AIConfigCategory.Instance.AIConfigs.ContainsKey(aiConfigId))
            {
                Log.Error($"[Monster] AIConfigId 不存在: {aiConfigId} unit={unit.Id}");
                return;
            }

            foreach (AIConfig node in AIConfigCategory.Instance.GetAI(aiConfigId).Values)
            {
                switch (node.Name)
                {
                    case "AI_Monster_XunLuo":
                        InitMonsterXunLuo(unit, spawnPos, node);
                        break;
                    default:
                        Log.Error($"[Monster] 未实现的 AI 节点: {node.Name} aiConfigId={aiConfigId}");
                        break;
                }
            }

            unit.AddComponent<AIComponent, int>(aiConfigId);
        }

        /// <summary>
        /// A 巡逻距离、B 巡逻间隔、C 是否追击、D 追击范围、E 追击休息、F 追击最近距离；
        /// A/B/D/E/F 配置 /1000（距离或时间）；C=0 时 D/E/F 按 0。
        /// </summary>
        private static void InitMonsterXunLuo(Unit unit, float3 spawnCenter, AIConfig node)
        {
            int paramA = node.NodeParams.Length > 0 ? (int)node.NodeParams[0] : 3000;
            int paramB = node.NodeParams.Length > 1 ? (int)node.NodeParams[1] : 1000;
            int paramC = node.NodeParams.Length > 2 ? (int)node.NodeParams[2] : 0;
            int paramD = 0;
            int paramE = 0;
            int paramF = 0;
            if (paramC == 1)
            {
                paramD = node.NodeParams.Length > 3 ? (int)node.NodeParams[3] : 0;
                paramE = node.NodeParams.Length > 4 ? (int)node.NodeParams[4] : 0;
                paramF = node.NodeParams.Length > 5 ? (int)node.NodeParams[5] : 0;
            }

            MonsterPatrolComponent patrol = unit.GetComponent<MonsterPatrolComponent>() ?? unit.AddComponent<MonsterPatrolComponent>();
            patrol.SpawnCenter = spawnCenter;
            patrol.PatrolRadiusM = paramA / 1000f;
            patrol.PatrolIntervalMs = ConfigTimeToMs(paramB);
            patrol.ChaseEnabled = paramC == 1;
            patrol.ChaseRangeM = paramC == 1 ? paramD / 1000f : 0f;
            patrol.ChaseRestMs = paramC == 1 ? ConfigTimeToMs(paramE) : 0;
            patrol.ChaseMinDistanceM = paramC == 1 ? paramF / 1000f : 0f;
            patrol.PatrolPhase = 0;
            patrol.MoveIntent = 0;
            patrol.ChaseTargetId = 0;
            patrol.NextMoveTime = TimeInfo.Instance.ServerNow();
        }

        private static int ConfigTimeToMs(int configValue)
        {
            return (int)(configValue / 1000f * 1000f);
        }
    }
}
