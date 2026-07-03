

using System.Collections.Generic;

namespace ET.Server
{
    public static partial class MapMessageHelper
    {
        private static bool CanSendClientMessage(Unit unit)
        {
            if (unit == null || unit.IsDisposed || !unit.IsPlayer())
            {
                return false;
            }

            Scene root = unit.Root();
            if (root == null || root.IsDisposed)
            {
                return false;
            }

            MessageLocationSenderComponent messageLocationSenderComponent = root.GetComponent<MessageLocationSenderComponent>();
            if (messageLocationSenderComponent == null || messageLocationSenderComponent.IsDisposed)
            {
                return false;
            }

            CoroutineLockComponent coroutineLockComponent = root.GetComponent<CoroutineLockComponent>();
            if (coroutineLockComponent == null || coroutineLockComponent.IsDisposed || coroutineLockComponent.IScene == null)
            {
                return false;
            }

            return true;
        }

        public static void NoticeUnitAdd(Unit unit, Unit sendUnit)
        {
            if (!CanSendClientMessage(unit) || sendUnit == null || sendUnit.IsDisposed)
            {
                return;
            }

            M2C_CreateUnits createUnits = M2C_CreateUnits.Create();
            createUnits.Units.Add(UnitHelper.CreateUnitInfo(sendUnit));
            MapMessageHelper.SendToClient(unit, createUnits).Coroutine();
        }
        
        public static void NoticeUnitRemove(Unit unit, Unit sendUnit)
        {
            if (!CanSendClientMessage(unit) || sendUnit == null || sendUnit.IsDisposed)
            {
                return;
            }

            M2C_RemoveUnits removeUnits = M2C_RemoveUnits.Create();
            removeUnits.Units.Add(sendUnit.Id);
            MapMessageHelper.SendToClient(unit, removeUnits).Coroutine();
        }
        
        public static void Broadcast(Unit unit, IMessage message)
        {
            (message as MessageObject).IsFromPool = false;
            Dictionary<long, EntityRef<AOIEntity>> dict = unit.GetBeSeePlayers();
            // 网络底层做了优化，同一个消息不会多次序列化
            MessageLocationSenderOneType oneTypeMessageLocationType = unit.Root().GetComponent<MessageLocationSenderComponent>().Get(LocationType.GateSession);
            foreach (AOIEntity u in dict.Values)
            {
                if (!u.Unit.IsPlayer())
                {
                    return;                    
                }

                oneTypeMessageLocationType.Send(u.Unit.Id, message).Coroutine();
            }
        }
        
        public static async ETTask SendToClient(this Unit unit, IMessage message)
        {
            if (!CanSendClientMessage(unit))
            {
                (message as MessageObject)?.Dispose();
                return;
            }

            await unit.Root().GetComponent<MessageLocationSenderComponent>().Get(LocationType.GateSession).Send(unit.Id, message);
        }
        
        
        
        /// <summary>
        /// 发送协议给Actor
        /// </summary>
        public static void Send(Scene root, ActorId actorId, IMessage message)
        {
            root.GetComponent<MessageSender>().Send(actorId, message);
        }
        
        public static void SendClient(Unit unit, IMessage message, NoticeClientType noticeClientType)
        {
            if (unit==null||unit.IsDisposed)
            {
                return;
            }
            
            if (message is IMapMessage iCurrentScene)
            {
                iCurrentScene.SceneId = unit.Scene().Id;
            }
            
            switch (noticeClientType)
            {
                case NoticeClientType.NoNotice:
                    break;
                case NoticeClientType.Self:
                    SendClientSelf(unit, message);
                    break;
                case NoticeClientType.Broadcast:
                    SendClientBroadcast(unit, message);
                    break;
                case NoticeClientType.BroadcastWithoutSelf:
                    SendClientBroadcastWithoutSelf(unit, message);
                    break;
            }
        }
        
        
        private static void SendClientSelf(Unit unit, IMessage message)
        {
            unit.Root().GetComponent<MessageLocationSenderComponent>().Get(LocationType.GateSession).Send(unit.Id, message).Coroutine();
        }
        
        private static void SendClientBroadcast(Unit unit, IMessage message)
        {
            if (unit.GetComponent<AOIEntity>() == null)
            {
                return;
            }
            (message as MessageObject).IsFromPool = false;
            Dictionary<long, EntityRef<AOIEntity>> dict = unit.GetBeSeePlayers();
            // 网络底层做了优化，同一个消息不会多次序列化
            MessageLocationSenderOneType oneTypeMessageLocationType = unit.Root().GetComponent<MessageLocationSenderComponent>().Get(LocationType.GateSession);
            foreach (AOIEntity u in dict.Values)
            {
                oneTypeMessageLocationType.Send(u.Unit.Id, message).Coroutine();
            }
        }

        
        private static void SendClientBroadcastWithoutSelf(Unit unit, IMessage message)
        {
            if (unit.GetComponent<AOIEntity>() == null)
            {
                return;
            }
            (message as MessageObject).IsFromPool = false;
            Dictionary<long, EntityRef<AOIEntity>> dict = unit.GetBeSeePlayers();
            // 网络底层做了优化，同一个消息不会多次序列化
            MessageLocationSenderOneType oneTypeMessageLocationType = unit.Root().GetComponent<MessageLocationSenderComponent>().Get(LocationType.GateSession);
            foreach (AOIEntity u in dict.Values)
            {
                if (unit.Id == u.Unit.Id)
                {
                    continue;
                }
                oneTypeMessageLocationType.Send(u.Unit.Id, message).Coroutine();
            }
        }
    }
}