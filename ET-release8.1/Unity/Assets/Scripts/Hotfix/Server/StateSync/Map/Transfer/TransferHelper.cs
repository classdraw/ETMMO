using System.Collections.Generic;
using MongoDB.Bson;

namespace ET.Server
{
    public static partial class TransferHelper
    {
        public static async ETTask TransferAtFrameFinish(Unit unit, ActorId sceneInstanceId, string sceneName,int mapConfigId, bool isEnterGame = false)
        {
            await unit.Fiber().WaitFrameFinish();

            await TransferHelper.Transfer(unit, sceneInstanceId, sceneName,mapConfigId,isEnterGame);
        }
        

        public static async ETTask Transfer(Unit unit, ActorId sceneInstanceId, string sceneName,int mapConfigId, bool isEnterGame = false)
        {
            Scene root = unit.Root();
            
            // location加锁
            long unitId = unit.Id;
            
            //传送就存档一份数据库
            unit.GetComponent<UnitDBSaveComponent>()?.SaveChangeNoWait();
            
            M2M_UnitTransferRequest request = M2M_UnitTransferRequest.Create();
            request.IsEnterGame = isEnterGame;
            request.MapId = mapConfigId;//测试 后面读表
            request.OldActorId = unit.GetActorId();
            request.Unit = unit.ToBson();
            
            //foreach (Entity entity in unit.Components.Values)
            //{
            //    if (entity is ITransfer)
            //    {
            //        request.Entitys.Add(entity.ToBson());
            //    }
            //}
            //
            //传送序列化存储
            foreach (var keyValuePair in unit.GetComponent<UnitDBSaveComponent>().Bytes)
            {
                request.Types.Add(keyValuePair.Key.FullName);
                request.Entitys.Add(keyValuePair.Value);
            }
            
            unit.Dispose();
            
            await root.GetComponent<LocationProxyComponent>().Lock(LocationType.Unit, unitId, request.OldActorId);
            await root.GetComponent<MessageSender>().Call(sceneInstanceId, request);
        }
    }
}