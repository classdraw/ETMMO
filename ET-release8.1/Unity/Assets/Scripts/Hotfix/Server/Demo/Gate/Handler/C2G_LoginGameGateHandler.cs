using System;
namespace ET.Server
{
    [FriendOf(typeof(RoleInfo))]
    [MessageSessionHandler(SceneType.Gate)]
    public class C2G_LoginGameGateHandler : MessageSessionHandler<C2G_LoginGameGate, G2C_LoginGameGate>
    {
        protected override async ETTask Run(Session session, C2G_LoginGameGate request, G2C_LoginGameGate response)
        {
            
            if (session.GetComponent<SessionLockingComponent>()!=null)
            {
                response.Error = ErrorCode.ERR_RequestRepeatedly;
                return;
            }

            Scene root = session.Root();
            string account = root.GetComponent<GateSessionKeyComponent>().Get(request.Key);
            if (account == null)
            {
                response.Error = ErrorCore.ERR_ConnectGateKeyError;
                response.Message = "Gate key验证失败!";
                session?.Disconnect().Coroutine();
                return;
            }
            //登陆
            root.GetComponent<GateSessionKeyComponent>().Remove(request.Key);
            //持续5秒 必须通过验证的组件 否则session dispose
            //SessionAcceptTimeoutComponent是防止外挂，链接后不验证也不干别的， 如果通过连接那么移除，否则5秒后这个session会释放
            session.RemoveComponent<SessionAcceptTimeoutComponent>();

            long instanceId = session.InstanceId;
            var coroutineLockComponent = session?.Root().GetComponent<CoroutineLockComponent>();
 
            using (session.AddComponent<SessionLockingComponent>()) //using 自动释放
            using (await coroutineLockComponent.Wait(CoroutineLockType.LoginGate, request.AccountName.GetLongHashCode()))
            {
                if (instanceId!=session.InstanceId)
                {
                    response.Error = ErrorCode.ERR_LoginGameGateError01;
                    return;
                }
                
                //通知登陆中心服操作
                G2L_AddLoginRecord g2LAddLoginRecord = G2L_AddLoginRecord.Create();
                g2LAddLoginRecord.AccountName = request.AccountName;
                g2LAddLoginRecord.ServerId = root.Zone();

                L2G_AddLoginRecord l2GAddLoginRecord=await root.GetComponent<MessageSender>().Call(StartSceneConfigCategory.Instance.LoginCenterConfig.ActorId, g2LAddLoginRecord)
                        as L2G_AddLoginRecord;
                if (l2GAddLoginRecord.Error!=ErrorCode.ERR_Success)
                {
                    response.Error = l2GAddLoginRecord.Error;
                    session?.Disconnect().Coroutine();
                    return;
                }
                PlayerComponent playerComponent = root.GetComponent<PlayerComponent>();
                Player player = playerComponent.GetByAccount(account);
                if (player == null)
                {
                    //通过player找到playerSessionComponent，再找到session 
                    //playerSessionComponent 可以网络消息处理
                    //player也可以网络消息处理 只是处理消息类型不同
                    //player的id和player的unitId一样
                    player = playerComponent.AddChildWithId<Player, string,int>(request.RoleId,account,request.BaseAvatar);
                    player.UnitId = request.RoleId;
                    
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

                    player.PlayerState = PlayerState.Gate;
                }
                else
                {
                    //这里是第二次登陆
                    player.RemoveComponent<PlayerOfflineOutTimeComponent>();//离线就需要增加这个组件
                    
                    //新的playerSession 挂上
                    session.AddComponent<SessionPlayerComponent>().Player = player;
                    player.GetComponent<PlayerSessionComponent>().Session = session;
                    //这里的playerState状态可能gate 可能map
                }
            
                response.PlayerId = player.Id;
            }
        }
    }
}

