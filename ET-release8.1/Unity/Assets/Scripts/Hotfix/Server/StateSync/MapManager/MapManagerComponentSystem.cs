using System.Collections.Generic;
using System.Linq;
using System;

namespace ET.Server
{
    
    [Invoke(TimerInvokeType.MapCloseCheckTimer)]
    public class MapCloseCheckTimerHandler : ATimer<MapManagerComponent>
    {
        protected override void Run(MapManagerComponent self)
        {
            try
            {
                self?.Check();
            }
            catch (Exception e)
            {
                Log.Error(e.ToString());
            }
        }
    }
    
    [EntitySystemOf(typeof (MapManagerComponent))]
    [FriendOf(typeof (MapUnit))]
    [FriendOf(typeof (MapManagerComponent))]
    public static partial class MapManagerComponentSystem
    {
        [EntitySystem]
        private static void Awake(this MapManagerComponent self)
        {
            long time = 1000;
            self.Timer = self.Root().GetComponent<TimerComponent>().NewRepeatedTimer(time, TimerInvokeType.MapCloseCheckTimer, self);
            self.InitAllMap().Coroutine();
        }
        
        [EntitySystem]
        private static void Destroy(this MapManagerComponent self)
        {
            self.Root().GetComponent<TimerComponent>().Remove(ref self.Timer);
        }
        /*----------------------------------------------------------------------------------------------------------------*/

        public static async ETTask<MapUnit> CreateMapAsync(this MapManagerComponent self, int mapConfigId, CreateMapCtx ctx = default)
        {
            MapConfig mapConfig = MapConfigCategory.Instance.Get(mapConfigId);
            if (mapConfig==null)
            {
                Log.Error($"找不到地图配置: {mapConfigId}");
                return null;
            }
            MapUnit mapUnit = self.AddChild<MapUnit, int>(mapConfigId);
            int id = await FiberManager.Instance.Create(SchedulerType.ThreadPool, self.Zone(), SceneType.Map, mapConfig.LogicName);
            mapUnit.fiberId = id;
            mapUnit.actorId=new ActorId(self.Fiber().Process, id);
            mapUnit.actorStr = mapUnit.actorId.ToString();
            long t = ctx.ExpiredTime > 0? ctx.ExpiredTime : mapConfig.ValidTime;
            if (t > 0)
            {
                mapUnit.closeTime = TimeInfo.Instance.FrameTime + t * 1000;//地图开始创建，那么销毁时间确定
                Log.Console($"[Map]地图{mapConfigId} 销毁时间{t*1000}");
            }
            else
            {
                mapUnit.closeTime = 0;
                Log.Console($"[Map]地图{mapConfigId} 无限制时间");
            }

            M2M_InitMap initMap = M2M_InitMap.Create();
            initMap.MapConfigId = mapConfigId;
            initMap.Ctx = ctx;
            self.Root().GetComponent<MessageSender>().Send(mapUnit.actorId, initMap);
            self.mapCfgDict[mapConfigId].Add(mapUnit.Id);
            Log.Console($"[Map]地图 {mapConfigId}创建成功, fiberId:{mapUnit.fiberId}");
            return mapUnit;
        }
        
        /// <summary>
        /// 玩家进入地图
        /// </summary>
        /// <param name="self"></param>
        /// <param name="message"></param>
        public static void EnterMap(this MapManagerComponent self,O2M_EnterMap message)
        {
            self.ExitMap(message.Id);
            foreach (var l in self.mapCfgDict[message.MapConfigId])
            {
                MapUnit mapUnit = self.GetChild<MapUnit>(l);
                if (mapUnit.actorId==message.MapActorId)
                {
                    mapUnit.AddCount();
                    self.roleMapDict.Add(message.Id,mapUnit.Id);
                    break;
                }
            }
        }
        
        /// <summary>
        /// 玩家离开地图
        /// </summary>
        /// <param name="self"></param>
        /// <param name="id"></param>
        private static void ExitMap(this MapManagerComponent self, long id)
        {
            if (self.roleMapDict.Remove(id, out long value))
            {
                MapUnit mapUnit = self.GetChild<MapUnit>(value);
                mapUnit.RemoveCount();
            }
        }

        public static async ETTask<(int, ActorId)> GetMapActorId(this MapManagerComponent self, int mapConfigId, long mapFiberId = 0)
        {
            using (await self.Root().GetComponent<CoroutineLockComponent>().Wait(CoroutineLockType.CreateMap, mapConfigId))
            {
                MapConfig config = MapConfigCategory.Instance.Get(mapConfigId);
                List<long> list = self.mapCfgDict[mapConfigId];//得到所有的MapUnit
                if (mapFiberId>0)
                {
                    foreach (long l in list)
                    {
                        MapUnit unit = self.GetChild<MapUnit>(l);
                        if (unit.fiberId == mapFiberId && unit.IsAvailable())
                        {
                            return (ErrorCode.ERR_Success, unit.actorId);
                        }
                    }
                }
                else
                {
                    //随机一个可用的
                    if (list.Count>0)
                    {
                        var ll = list.Select(self.GetChild<MapUnit>).Where(unit => unit.IsAvailable()).ToList();
                        if (ll.Count > 0)
                        {
                            var unit = RandomGenerator.RandomArray(ll);
                            Log.Console($"[Map]随机一个已有地图MapConfigId:{mapConfigId} ActorId:{unit.actorId}");
                            return (ErrorCode.ERR_Success, unit.actorId);
                        }
                        //能分区就再创建一个
                        if (config.Div)
                        {
                            MapUnit createUnit = await self.CreateMapAsync(mapConfigId);
                            return (ErrorCode.ERR_Success, createUnit.actorId);
                        }
                    }
                    else
                    {
                        MapUnit createUnit = await self.CreateMapAsync(mapConfigId);
                        return (ErrorCode.ERR_Success, createUnit.actorId);
                    }
                }
            }

            return (ErrorCode.ERR_EnterMapError, default);
        }

        /// <summary>
        /// 获取玩家当前所在的地图
        /// </summary>
        /// <param name="self"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public static ActorId GetUnitActorId(this MapManagerComponent self, long id)
        {
            if (!self.roleMapDict.TryGetValue(id, out long mapConfigId))
            {
                return default;
            }

            return self.GetChild<MapUnit>(mapConfigId).actorId;
        }
        
        
        public static void Check(this MapManagerComponent self)
        {
            using var list = ListComponent<long>.Create();
            foreach ((int cfgId, var ll) in self.mapCfgDict)
            {
                foreach (long l in ll)
                {
                    MapUnit unit = self.GetChild<MapUnit>(l);
                    if (unit.closeTime > 0 &&unit.validTime==0&& TimeInfo.Instance.FrameTime >= unit.closeTime)
                    {
                        //15S后销毁
                        unit.validTime = unit.closeTime + 15000;
                        Log.Console($"[Map]地图MapId: {unit.MapConfigId}, Id: {unit.Id} ActorId:{unit.actorId} 准备{15000}毫秒后关闭");
                        self.ForceTransferPlayersFromMap(unit).Coroutine();
                    }

                    if (unit.validTime > 0 && TimeInfo.Instance.FrameTime >= unit.validTime)
                    {
                        list.Add(l);
                    }
                }

                MapConfig config = MapConfigCategory.Instance.Get(cfgId);
                if (!config.Div)
                {
                    continue;
                }

                for (int i = 1; i < ll.Count; i++)
                {
                    MapUnit unit = self.GetChild<MapUnit>(ll[i]);
                    if (unit.count == 0)
                    {
                        list.Add(ll[i]);
                    }
                }
            }

            foreach (long id in list)
            {
                self.Remove(id);
            }
        }
        
        /// <summary>
        /// 地图进入关闭倒计时，强制将地图内玩家传送到默认地图
        /// </summary>
        private static async ETTask ForceTransferPlayersFromMap(this MapManagerComponent self, MapUnit unit)
        {
            int defaultMapId = TransferHelper.GetDefaultScene();//后续可能有逻辑，比如我在C地图，传送出来默认是B地图，我在M地图，传送出来默认是K地图
            (int errno, ActorId targetActorId) = await self.GetMapActorId(defaultMapId);
            if (errno != ErrorCode.ERR_Success)
            {
                Log.Error($"[Map]地图关闭传送失败 找不到默认地图: {defaultMapId}, MapUnitId: {unit.Id}");
                return;
            }

            if (unit.count <= 0)
            {
                return;
            }

            Log.Console($"[Map]地图MapId: {unit.MapConfigId}, Id: {unit.Id} 强制传送 {unit.count} 名玩家到默认地图: {defaultMapId}");

            M2M_MapCloseTransfer message = M2M_MapCloseTransfer.Create();
            message.MapConfigId = defaultMapId;
            message.TargetActorId = targetActorId;
            self.Root().GetComponent<MessageSender>().Send(unit.actorId, message);
        }

        private static void Remove(this MapManagerComponent self, long id)
        {
            MapUnit unit = self.GetChild<MapUnit>(id);
            Log.Console($"[Map]地图MapId: {unit.MapConfigId}, Id: {unit.Id} 正式销毁");
            using var roleIds = ListComponent<long>.Create();
            foreach ((long roleId, long mapUnitId) in self.roleMapDict)
            {
                if (mapUnitId == id)
                {
                    roleIds.Add(roleId);
                }
            }

            foreach (long roleId in roleIds)
            {
                self.roleMapDict.Remove(roleId);
            }

            FiberManager.Instance.Remove(unit.fiberId).Coroutine();
            if (self.mapCfgDict.TryGetValue(unit.MapConfigId, out var list))
            {
                list.Remove(id);
            }

            unit.Dispose();
        }


        private static async ETTask InitAllMap(this MapManagerComponent self)
        {
            foreach (var kvp in MapConfigCategory.Instance.GetAll())
            {
                self.mapCfgDict[kvp.Key] = new List<long>();
                if (kvp.Value.AutoCreate)//自动创建
                {
                    
                }
            }

            await ETTask.CompletedTask;
        }
    }
}

