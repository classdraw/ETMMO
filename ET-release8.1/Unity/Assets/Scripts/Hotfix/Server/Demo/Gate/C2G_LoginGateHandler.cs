using System;


namespace ET.Server
{
    [MessageSessionHandler(SceneType.Gate)]
    public class C2G_LoginGateHandler : MessageSessionHandler<C2G_LoginGate, G2C_LoginGate>
    {
        protected override async ETTask Run(Session session, C2G_LoginGate request, G2C_LoginGate response)
        {
            Scene root = session.Root();
            string account = root.GetComponent<GateSessionKeyComponent>().Get(request.Key);
            if (account == null)
            {
                response.Error = ErrorCore.ERR_ConnectGateKeyError;
                response.Message = "Gate key验证失败!";
                return;
            }
            //SessionAcceptTimeoutComponent是防止外挂，链接后不验证也不干别的， 如果通过连接那么移除，否则5秒后这个session会释放
            session.RemoveComponent<SessionAcceptTimeoutComponent>();

            PlayerComponent playerComponent = root.GetComponent<PlayerComponent>();
            Player player = playerComponent.GetByAccount(account);
            if (player == null)
            {
                //通过player找到playerSessionComponent，再找到session 
                //playerSessionComponent 可以网络消息处理
                //player也可以网络消息处理 只是处理消息类型不同
                player = playerComponent.AddChild<Player, string>(account);
                playerComponent.Add(player);
                //每个玩家保存一个玩家电话组件 用于通信
                PlayerSessionComponent playerSessionComponent = player.AddComponent<PlayerSessionComponent>();
                //playerSession拥有处理网络消息能力
                playerSessionComponent.AddComponent<MailBoxComponent, MailBoxType>(MailBoxType.GateSession);
                //通知定位服务器我们这个playerSession位置
                await playerSessionComponent.AddLocation(LocationType.GateSession);
			    //player这个组件可以处理网络消息的能力
                player.AddComponent<MailBoxComponent, MailBoxType>(MailBoxType.UnOrderedMessage);
                //通知location定位服务器 我这个player实体所在具体位置
                await player.AddLocation(LocationType.Player);
			    //和这个玩家通信的session
                session.AddComponent<SessionPlayerComponent>().Player = player;
                playerSessionComponent.Session = session;
            }
            else
            {
                // 判断是否在战斗
                //帧同步 房间匹配
                PlayerRoomComponent playerRoomComponent = player.GetComponent<PlayerRoomComponent>();
                if (playerRoomComponent.RoomActorId != default)
                {
                    CheckRoom(player, session).Coroutine();
                }
                else
                {
                    //新的playerSession 挂上
                    PlayerSessionComponent playerSessionComponent = player.GetComponent<PlayerSessionComponent>();
                    playerSessionComponent.Session = session;
                }
            }

            response.PlayerId = player.Id;
            await ETTask.CompletedTask;
        }

        private static async ETTask CheckRoom(Player player, Session session)
        {
            Fiber fiber = player.Fiber();
            await fiber.WaitFrameFinish();

            G2Room_Reconnect g2RoomReconnect = G2Room_Reconnect.Create();
            g2RoomReconnect.PlayerId = player.Id;
            using Room2G_Reconnect room2GateReconnect = await fiber.Root.GetComponent<MessageSender>().Call(
                player.GetComponent<PlayerRoomComponent>().RoomActorId,
                g2RoomReconnect) as Room2G_Reconnect;
            G2C_Reconnect g2CReconnect = G2C_Reconnect.Create();
            g2CReconnect.StartTime = room2GateReconnect.StartTime;
            g2CReconnect.Frame = room2GateReconnect.Frame;
            g2CReconnect.UnitInfos.AddRange(room2GateReconnect.UnitInfos);
            session.Send(g2CReconnect);
            
            session.AddComponent<SessionPlayerComponent>().Player = player;
            player.GetComponent<PlayerSessionComponent>().Session = session;
        }
    }
}