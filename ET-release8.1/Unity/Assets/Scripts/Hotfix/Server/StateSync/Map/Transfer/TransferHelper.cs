using System;
using System.Collections.Generic;

namespace ET.Server
{
    public static partial class TransferHelper
    {
        public static int GetDefaultScene()
        {
            if (!ConstantConfigCategory.Instance.Contain(ConstantConfigKeys.ConstantDefaultMap))
            {
                Log.Warning($"[Map]ConstantConfig 缺少 Id={ConstantConfigKeys.ConstantDefaultMap}，背包上限使用回退值 500");
                return 10001;
            }

            return ConstantConfigCategory.Instance.Get(ConstantConfigKeys.ConstantDefaultMap).IntValue;
        }


        public static async ETTask<(int, ActorId, int)> GetValidMap(Scene scene, Unit unit, int layer = 1)
        {
            List<int[]> tryEnterList = new List<int[]>();
            //[unit.MapId, unit.MapUid], [unit.LastMapId, 0], [ConstValue.StartMap, 0]
            tryEnterList.Add(new []{unit.MapId, unit.MapUid});
            tryEnterList.Add(new []{unit.LastMapId, 0});
            int defaultScene = GetDefaultScene();
            tryEnterList.Add(new []{defaultScene, 0});
            for (int i = Math.Min(layer, tryEnterList.Count); i < tryEnterList.Count; i++)
            {
                int[] item = tryEnterList[i];
                int mapId = item[0];
                if (mapId <= 0)
                {
                    continue;
                }

                (int errno, ActorId mapActorId) r = await MapManagerHelper.GetMapActorId(scene, mapId, item[1]);
                if (r.errno != ErrorCode.ERR_Success)
                {
                    Log.Console("[Map]地图都满员或者不让创建地图");
                    continue;
                }
                Log.Console($"[Map]找到可以进入的地图,MapActorId:{r.mapActorId}__ConfigId:{mapId}__UnitId:{unit.Id}");
                return (r.errno, r.mapActorId, mapId);
            }
            Log.Error($"[Map]我X 卡玩家了, {unit.Id}");
            return (ErrorCode.ERR_EnterMapError, default, 0);
        }

        public static async ETTask TransferAtFrameFinish(Unit unit, ActorId sceneInstanceId,int mapConfigId, bool isEnterGame = false)
        {
            await unit.Fiber().WaitFrameFinish();

            await TransferHelper.Transfer(unit, sceneInstanceId,mapConfigId,isEnterGame);
        }
        

        public static async ETTask Transfer(Unit unit, ActorId sceneInstanceId,int mapConfigId, bool isEnterGame = false)
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