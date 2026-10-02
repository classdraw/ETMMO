using Unity.Mathematics;

namespace ET.Server
{
    public static class RobotSpawnHelper
    {
        public const int DefaultGameZone = 1;

        public static bool TryParseAutoSkillArgs(string commandLine, out int count, out int mapConfigId)
        {
            count = 0;
            mapConfigId = 0;
            string[] strs = commandLine.Split(' ');
            if (strs.Length != 4 || strs[0] != "Run" || strs[1] != "3")
            {
                return false;
            }

            count = int.Parse(strs[2]);
            mapConfigId = int.Parse(strs[3]);
            return count > 0 && mapConfigId >= 10001;
        }

        public static RobotSpawnContext BuildSpawnContext(int mapConfigId)
        {
            MapConfig mapConfig = MapConfigCategory.Instance.Get(mapConfigId);
            Log.Console($"[Robot] 地图 mapConfigId={mapConfigId} ({mapConfig.LogicName}) 出生基准=({RobotUnitHelper.RobotSpawnBaseX},{RobotUnitHelper.RobotSpawnBaseY},{RobotUnitHelper.RobotSpawnBaseZ})");
            return new RobotSpawnContext { MapConfigId = mapConfigId, AiConfigId = RobotUnitHelper.RunCase3AiConfigId };
        }

        public static float3 RandomSpawnPosition()
        {
            return RandomPositionAround(
                RobotUnitHelper.RobotSpawnBaseX,
                RobotUnitHelper.RobotSpawnBaseY,
                RobotUnitHelper.RobotSpawnBaseZ,
                RobotUnitHelper.RobotSpawnRadiusMm);
        }

        public static float3 RandomPositionAround(float baseX, float baseY, float baseZ, int radiusMm)
        {
            float dx = RandomGenerator.RandomNumber(-radiusMm, radiusMm) / 1000f;
            float dz = RandomGenerator.RandomNumber(-radiusMm, radiusMm) / 1000f;
            return new float3(baseX + dx, baseY, baseZ + dz);
        }

        /// <summary>向地图服 Inner 刷压测 Robot（9101 + 服务端 Brain）。</summary>
        public static async ETTask<int> SpawnPressureRobotsAsync(Scene root, int mapConfigId, int count)
        {
            (int errno, ActorId mapActorId) = await MapManagerHelper.GetMapActorId(root, mapConfigId, 0, DefaultGameZone);
            if (errno != ErrorCode.ERR_Success)
            {
                Log.Console($"[Robot] 获取地图分线失败 map={mapConfigId} err={errno}");
                return 0;
            }

            O2M_RobotSpawnPressureMonstersRequest request = O2M_RobotSpawnPressureMonstersRequest.Create();
            request.MonsterConfigId = RobotUnitHelper.RunCase3AiConfigId;
            request.Count = count;
            request.BaseX = RobotUnitHelper.RobotSpawnBaseX;
            request.BaseY = RobotUnitHelper.RobotSpawnBaseY;
            request.BaseZ = RobotUnitHelper.RobotSpawnBaseZ;
            request.RadiusMm = RobotUnitHelper.RobotSpawnRadiusMm;

            M2O_RobotSpawnPressureMonstersResponse response =
                await root.GetComponent<MessageSender>().Call(mapActorId, request) as M2O_RobotSpawnPressureMonstersResponse;
            if (response == null || response.Error != ErrorCode.ERR_Success)
            {
                int err = response?.Error ?? ErrorCode.ERR_None;
                Log.Console($"[Robot] 刷 Robot 失败 map={mapConfigId} err={err} msg={response?.Message}");
                return 0;
            }

            return response.SpawnedCount;
        }
    }
}
