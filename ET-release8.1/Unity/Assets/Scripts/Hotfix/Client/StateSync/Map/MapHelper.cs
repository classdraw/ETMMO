namespace ET.Client
{
    public static class MapHelper
    {
        public static async ETTask<int> GMTransferMap(Scene root, int mapConfigId)
        {
            return await TransferMapAsync(root, mapConfigId, 0);
        }

        /// <summary>
        /// 传送到指定地图；mapFiberId&gt;0 时进入与主角相同的分线实例。
        /// </summary>
        public static async ETTask<int> TransferMapAsync(Scene root, int mapConfigId, long mapFiberId = 0)
        {
            bool robot = RobotUnitHelper.IsRobotAccount(root.Name);
            if (robot)
            {
                RobotUnitHelper.LogConsole(root.Name, $"TransferMapAsync 请求 map={mapConfigId} fiber={mapFiberId}");
            }

            C2M_TransferMap c2MTransferMap = C2M_TransferMap.Create();
            c2MTransferMap.MapConfigId = mapConfigId;
            c2MTransferMap.MapFiberId = mapFiberId;
            M2C_TransferMap m2CTransferMap = await root.GetComponent<ClientSenderComponent>().Call(c2MTransferMap) as M2C_TransferMap;
            if (m2CTransferMap == null)
            {
                Log.Error($"[Map] 传送无响应 mapConfigId={mapConfigId}");
                return ErrorCode.ERR_None;
            }

            if (m2CTransferMap.Error != ErrorCode.ERR_Success)
            {
                Log.Error($"[Map] 传送失败 mapConfigId={mapConfigId} fiber={mapFiberId} err={m2CTransferMap.Error}");
                return m2CTransferMap.Error;
            }

            await root.GetComponent<ObjectWait>().Wait<Wait_SceneChangeFinish>();
            if (robot)
            {
                RobotUnitHelper.LogConsole(root.Name, $"TransferMapAsync 切场景完成 map={mapConfigId} curScene={root.CurrentScene()?.Name}");
            }

            Log.Info($"[Map] 传送成功 mapConfigId={mapConfigId} fiber={mapFiberId}");
            return ErrorCode.ERR_Success;
        }
    }
}

