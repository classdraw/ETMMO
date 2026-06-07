namespace ET.Client
{
    public static class MapHelper
    {
        public static async ETTask<int> GMTransferMap(Scene root, int mapConfigId)
        {
            
            C2M_TransferMap c2MTransferMap = C2M_TransferMap.Create();
            c2MTransferMap.MapConfigId = mapConfigId;
            c2MTransferMap.MapFiberId = 0;//去指定分区地图用到
            M2C_TransferMap m2CTransferMap=await root.GetComponent<ClientSenderComponent>().Call(c2MTransferMap) as M2C_TransferMap;
            if (m2CTransferMap.Error==ErrorCode.ERR_Success)
            {
                Log.Info($"传送地图 mapConfigId={mapConfigId} 成功!!!");
            }
            return m2CTransferMap.Error;
        }

    }
}

