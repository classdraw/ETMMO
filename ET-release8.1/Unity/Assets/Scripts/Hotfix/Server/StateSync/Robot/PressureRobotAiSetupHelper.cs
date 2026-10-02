using Unity.Mathematics;

namespace ET.Server
{
    /// <summary>
    /// 压测机器人：与怪物共用 AI 管线（Unit 上挂 AIComponent + AIConfig 多节点）。
    /// </summary>
    [FriendOfAttribute(typeof(ET.XunLuoPathComponent))]    
    public static class PressureRobotAiSetupHelper
    {
        public static void Setup(Unit unit, int aiConfigId, float3 patrolCenter)
        {
            if (!AIConfigCategory.Instance.AIConfigs.ContainsKey(aiConfigId))
            {
                Log.Error($"[Robot] AIConfigId 不存在: {aiConfigId} unit={unit.Id}");
                return;
            }

            foreach (AIConfig node in AIConfigCategory.Instance.GetAI(aiConfigId).Values)
            {
                switch (node.Name)
                {
                    case "AI_XunLuo":
                        InitXunLuoPath(unit, patrolCenter);
                        break;
                    case "AI_AutoSkill":
                        if (unit.GetComponent<AutoSkillComponent>() == null)
                        {
                            unit.AddComponent<AutoSkillComponent>();
                        }
                        break;
                }
            }

            unit.AddComponent<AIComponent, int>(aiConfigId);
            Log.Console($"[Robot] 挂载 AIComponent aiConfigId={aiConfigId} unit={unit.Id} nodes={AIConfigCategory.Instance.GetAI(aiConfigId).Count}");
        }

        private static void InitXunLuoPath(Unit unit, float3 center)
        {
            XunLuoPathComponent path = unit.GetComponent<XunLuoPathComponent>() ?? unit.AddComponent<XunLuoPathComponent>();
            int length = RandomGenerator.RandomNumber(5, 10);
            path.path = new float3[length];
            for (int i = 0; i < length; i++)
            {
                path.path[i] = center + new float3(
                    RandomGenerator.RandomNumber(-300, 300) / 100f,
                    0,
                    RandomGenerator.RandomNumber(-300, 300) / 100f);
            }

            path.Index = 0;
            path.PatrolPhase = 0;
            path.NextMoveTime = TimeInfo.Instance.ServerNow();
        }
    }
}
