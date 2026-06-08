using System;
using System.Collections.Generic;
using Unity.Mathematics;

namespace ET.Server
{
    [FriendOf(typeof(MoveComponent))]
    [FriendOf(typeof(NumericComponent))]
    [FriendOf(typeof(UnitDBSaveComponent))]
    public static partial class UnitHelper
    {
        /// <summary>
        /// 强制从当前地图下线：移除 AOI → 触发 <see cref="UnitOfflinePersist"/>（写库等）→ 下一帧后摘 Location、GateSession 映射并 Dispose Unit。
        /// 与 <see cref="G2M_RequestExitGameHandler"/> / <see cref="G2M_SessionDisconnectHandler"/> 共用。
        /// </summary>
        public static async ETTask ForceUnitOfflineFromMapAsync(Unit unit, string reasonTag = null)
        {
            if (unit == null || unit.IsDisposed)
            {
                return;
            }

            string tag = string.IsNullOrEmpty(reasonTag) ? "Offline" : reasonTag;
            Log.Console($"ForceUnitOfflineFromMap [{tag}] roleId:{unit.Id}");
            unit.RemoveComponent<AOIEntity>();
            UnitDBSaveComponent dbSave = unit.GetComponent<UnitDBSaveComponent>();
            if (dbSave != null)
            {
                await dbSave.SaveChange();
            }
            await EventSystem.Instance.PublishAsync(unit.Scene(), new UnitOfflinePersist { Unit = unit });
            await unit.Fiber().WaitFrameFinish();
            await unit.RemoveLocation(LocationType.Unit);
            unit.Root().GetComponent<MessageLocationSenderComponent>().Get(LocationType.GateSession).Remove(unit.Id);
            UnitComponent unitComponent = unit.Root().GetComponent<UnitComponent>();
            unitComponent.Remove(unit.Id);
        }
        
        public static void AfterTransfer(Unit unit,Scene root, M2M_UnitTransferRequest request)
        {
            /**
            unit.AddComponent<MoveComponent>();
            unit.AddComponent<PathfindingComponent, string>(MapConfigCategory.Instance.Get(request.MapId).PathName);
            unit.AddComponent<MailBoxComponent, MailBoxType>(MailBoxType.OrderedMessage);
            unit.AddComponent<SummonComponent>();
            if (request.IsEnterGame)
            {
                unit.AddComponent<NumericComponent>();
                unit.AddComponent<FashionComponent>();

                EventSystem.Instance.Publish(unit.Scene(), new UnitCheckCfg() { Unit = unit });
                EventSystem.Instance.Publish(unit.Scene(), new UnitReEffect() { Unit = unit });
            }*/
            
            unit.AddComponent<NumericNoticeComponent>();//数值同步组件
            unit.AddComponent<MoveComponent>();
            unit.AddComponent<PathfindingComponent, string>(root.Name);
            unit.AddComponent<MailBoxComponent, MailBoxType>(MailBoxType.OrderedMessage);


            SetUnitDataAfterTransfer(unit,request.MapId);
            
            if (request.IsEnterGame)
            {

                EventSystem.Instance.Publish(unit.Scene(), new UnitCheckCfg() { Unit = unit });
                EventSystem.Instance.Publish(unit.Scene(), new UnitReEffect() { Unit = unit });
            }
        }

        private static void SetUnitDataAfterTransfer(Unit unit,int mapConfigId)
        {
            MapConfig mapConfig = MapConfigCategory.Instance.Get(mapConfigId);
            float[] startPoint = mapConfig.StartPoint;
            unit.Position = new float3(startPoint[0], startPoint[1], startPoint[2]);
        }

        /// <summary>
        /// 处理离线消息
        /// </summary>
        /// <param name="unit"></param>
        public static async ETTask DealOfflineMsg(Unit unit)
        {
            /**
            List<OfflineUnit> offlineUnits = await MapManagerHelper.GetOfflineUnits(unit.Scene(), unit.Id);
            foreach (OfflineUnit offlineUnit in offlineUnits)
            {
                await OfflineMsgDispatcher.Instance.Run(unit, offlineUnit.Message, offlineUnit.Args);
            }*/
            await ETTask.CompletedTask;
        }
        
        public static UnitInfo CreateUnitInfo(Unit unit)
        {
            UnitInfo unitInfo = UnitInfo.Create();
            NumericComponent nc = unit.GetComponent<NumericComponent>();
            unitInfo.UnitId = unit.Id;
            unitInfo.Name = unit.Name;
            unitInfo.ConfigId = unit.ConfigId;
            unitInfo.Type = (int)unit.Type();
            unitInfo.OwnerId = unit.OwnerId;
            unitInfo.TeamId = unit.TeamId;
            unitInfo.Position = unit.Position;
            unitInfo.Forward = unit.Forward;

            MoveComponent moveComponent = unit.GetComponent<MoveComponent>();
            if (moveComponent != null)
            {
                if (!moveComponent.IsArrived())
                {
                    unitInfo.MoveInfo = MoveInfo.Create();
                    unitInfo.MoveInfo.Points.Add(unit.Position);
                    for (int i = moveComponent.N; i < moveComponent.Targets.Count; ++i)
                    {
                        float3 pos = moveComponent.Targets[i];
                        unitInfo.MoveInfo.Points.Add(pos);
                    }
                }
            }

            if (nc!=null&&nc.NumericDic!=null)
            {
                foreach ((int key, long value) in nc.NumericDic)
                {
                    unitInfo.KV.Add(key, value);
                }
            }



            return unitInfo;
        }
        
        // 获取看见unit的玩家，主要用于广播
        public static Dictionary<long, EntityRef<AOIEntity>> GetBeSeePlayers(this Unit self)
        {
            return self.GetComponent<AOIEntity>().GetBeSeePlayers();
        }
        
        public static void ChangeMap(Unit unit,int mapConfigId,int mapFiberId)
        {
            unit.LastMapId = unit.MapId;
            unit.MapId = mapConfigId;
            unit.MapUid = mapFiberId;
            unit.GetComponent<UnitDBSaveComponent>()?.MarkUnitDirty();
        }
    }
}