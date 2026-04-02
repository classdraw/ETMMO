using System.Collections.Generic;
using Unity.Mathematics;

namespace ET.Server
{
    [FriendOf(typeof(MoveComponent))]
    [FriendOf(typeof(NumericComponent))]
    public static partial class UnitHelper
    {
        
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
            
            unit.AddComponent<MoveComponent>();
            unit.AddComponent<PathfindingComponent, string>(root.Name);
            unit.Position = new float3(-10, 0, -10);

            unit.AddComponent<MailBoxComponent, MailBoxType>(MailBoxType.OrderedMessage);
            if (request.IsEnterGame)
            {
                //unit.AddComponent<NumericComponent>();
                //unit.AddComponent<FashionComponent>();

                EventSystem.Instance.Publish(unit.Scene(), new UnitCheckCfg() { Unit = unit });
                EventSystem.Instance.Publish(unit.Scene(), new UnitReEffect() { Unit = unit });
            }
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
            unitInfo.ConfigId = unit.ConfigId;
            unitInfo.Type = (int)unit.Type();
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

            foreach ((int key, long value) in nc.NumericDic)
            {
                unitInfo.KV.Add(key, value);
            }

            return unitInfo;
        }
        
        // 获取看见unit的玩家，主要用于广播
        public static Dictionary<long, EntityRef<AOIEntity>> GetBeSeePlayers(this Unit self)
        {
            return self.GetComponent<AOIEntity>().GetBeSeePlayers();
        }
    }
}