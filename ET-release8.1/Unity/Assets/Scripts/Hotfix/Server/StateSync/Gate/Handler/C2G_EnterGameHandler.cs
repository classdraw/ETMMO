using System;

namespace ET.Server
{
    [MessageSessionHandler(SceneType.Gate)]
    public class C2G_EnterGameHandler : MessageSessionHandler<C2G_EnterGame, G2C_EnterGame>
    {
        protected override async ETTask Run(Session session, C2G_EnterGame request, G2C_EnterGame response)
        {
            if (session.GetComponent<SessionLockingComponent>()!=null)
            {
                response.Error = ErrorCode.ERR_RequestRepeatedly;
                return;
            }

            SessionPlayerComponent sessionPlayerComponent = session.GetComponent<SessionPlayerComponent>();
            if (sessionPlayerComponent==null)
            {
                response.Error = ErrorCode.ERR_SessionPlayerError;
                return;
            }

            Player player = sessionPlayerComponent.Player;
            if (player==null||player.IsDisposed)
            {
                response.Error = ErrorCode.ERR_NonePlayerError;
                return;
            }
            
            var coroutineLockComponent = session?.Root().GetComponent<CoroutineLockComponent>();
            long instanceId = session.InstanceId;
            using (session.AddComponent<SessionLockingComponent>()) //using 自动释放
            using (await coroutineLockComponent.Wait(CoroutineLockType.LoginGate, player.AccountName.GetLongHashCode()))
            {
                //换玩家了
                if (instanceId!=session.InstanceId||player.IsDisposed)
                {
                    response.Error = ErrorCode.ERR_PlayerSessionError;
                    return;
                }
                
                if (player.PlayerState==PlayerState.Game)
                {
                    //二次登陆
                    try
                    {
                        G2M_SecondLogin g2MSecondLogin = G2M_SecondLogin.Create();
                        var m2GSecondLogin = (M2G_SecondLogin)await session.Root().GetComponent<MessageLocationSenderComponent>().Get(LocationType.Unit)
                                .Call(player.UnitId, g2MSecondLogin);
                        if (m2GSecondLogin.Error==ErrorCode.ERR_Success)
                        {
                            response.MyUnitId = player.UnitId;
                            return;
                        }

                        string errMsg = $"二次登陆失败 Error：{m2GSecondLogin.Error} Message：{m2GSecondLogin.Message}";
                        Log.Error(errMsg);
                        response.Error = ErrorCode.ERR_RepeatedEnterGameError1;
                        await DisconnectHelper.KickPlayerNoLock(player);
                        session?.Disconnect().Coroutine();
                    }
                    catch(Exception ex)
                    {
                        Log.Console($"角色进入游戏逻辑出现问题 账号:{player.AccountName} 角色:{player.UnitId} 异常信息:{ex}");
                        response.Error = ErrorCode.ERR_RepeatedEnterGameError2;
                        await DisconnectHelper.KickPlayerNoLock(player);
                        session?.Disconnect().Coroutine();
                    }
                }
                else
                {
                    Unit unit = null;
                    try
                    {

                        // 在Gate上动态创建一个Map Scene，把Unit从DB中加载放进来，然后传送到真正的Map中，这样登陆跟传送的逻辑就完全一样了
                       // GateMapComponent gateMapComponent = player.AddComponent<GateMapComponent>();
                        //gateMapComponent.Scene = await GateMapFactory.Create(gateMapComponent, player.Id, IdGenerater.Instance.GenerateInstanceId(), "GateMap");

                        //Scene scene = gateMapComponent.Scene;

                        // 这里可以从DB中加载Unit
                       // unit = UnitFactory.Create(scene, player.Id, UnitType.Player);
                       // long unitId = unit.Id;

                       (bool isNewPlayer,Unit unit1) = await UnitLoadHelper.LoadUnit(player);
                       unit = unit1;
                       
                        //登陆邮箱服务器
                        long unitId = unit.Id;
                        
                        StartSceneConfig startSceneConfig = StartSceneConfigCategory.Instance.GetBySceneName(session.Zone(), "Map1");
                        // 等到一帧的最后面再传送，先让G2C_EnterMap返回，否则传送消息可能比G2C_EnterMap还早
                        TransferHelper.TransferAtFrameFinish(unit, startSceneConfig.ActorId, startSceneConfig.Name,true).Coroutine();

                        player.UnitId = unitId;
                        response.MyUnitId = unitId;
                        player.PlayerState = PlayerState.Game;//这里表示已经进入mapScene
                    }
                    catch (Exception ex)
                    {
                        Log.Error($"角色进入游戏逻辑出现问题 账号:{player.AccountName} 角色Id:{player.Id} 异常信息{ex}");
                        unit?.Dispose();
                        response.Error = ErrorCode.ERR_ErrorEnterGame;
                        await DisconnectHelper.KickPlayerNoLock(player);
                        session.Disconnect().Coroutine();
                    }

                }

            }
            
            
            /*

                    */
        }
    }
}

