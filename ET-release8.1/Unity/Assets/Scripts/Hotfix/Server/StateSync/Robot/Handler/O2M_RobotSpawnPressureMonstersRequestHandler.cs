using Unity.Mathematics;

namespace ET.Server
{
    /// <summary>
    /// Process=2 压测：在地图服批量刷 Robot（9101），Inner 消息名保留兼容。
    /// </summary>
    [MessageHandler(SceneType.Map)]
    public class O2M_RobotSpawnPressureMonstersRequestHandler : MessageHandler<Scene, O2M_RobotSpawnPressureMonstersRequest, M2O_RobotSpawnPressureMonstersResponse>
    {
        protected override async ETTask Run(Scene scene, O2M_RobotSpawnPressureMonstersRequest request, M2O_RobotSpawnPressureMonstersResponse response)
        {
            MonsterMapComponent monsterMap = scene.GetComponent<MonsterMapComponent>();
            if (monsterMap == null)
            {
                response.Error = ErrorCode.ERR_EnterMapError;
                response.Message = "MonsterMapComponent missing";
                return;
            }

            int spawned = 0;
            for (int i = 0; i < request.Count; i++)
            {
                float3 pos = RobotSpawnHelper.RandomPositionAround(
                    request.BaseX, request.BaseY, request.BaseZ, request.RadiusMm);
                string name = $"Robot_{spawned + 1}";
                int aiConfigId = request.MonsterConfigId > 0 ? request.MonsterConfigId : RobotUnitHelper.RunCase3AiConfigId;
                Unit unit = monsterMap.CreatePressureRobotAt(pos, aiConfigId, name);
                if (unit != null)
                {
                    spawned++;
                }
            }

            response.SpawnedCount = spawned;
            response.Error = spawned > 0 ? ErrorCode.ERR_Success : ErrorCode.ERR_EnterMapError;
            Log.Console($"[Robot][Map] 刷压测 Robot spawned={spawned}/{request.Count} cfg={RobotUnitHelper.RobotUnitConfigId} scene={scene.Name}");
            await ETTask.CompletedTask;
        }
    }
}
