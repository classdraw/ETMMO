namespace ET.Server
{
    public static class DisconnectHelper
    {
        //延迟1秒，如果instanceId不同表示被复用了 如果相同断开连接
        public static async ETTask Disconnect(this Session self)
        {
            if (self==null||self.IsDisposed)
            {
                return;
            }

            long instanceId = self.InstanceId;
            TimerComponent timerComponent = self.Root().GetComponent<TimerComponent>();
            await timerComponent.WaitAsync(1000);
            if (self.InstanceId != instanceId)//被释放复用
            {
                return;
            }
            self.Dispose();
        }

        private static async ETTask KickPlayerNoLock(Player player)
        {
            if (player==null||player.IsDisposed)
            {
               return; 
            }

            switch (player.PlayerState)
            {
                case PlayerState.Disconnect:
                    break;
                case PlayerState.Gate:
                    break;
                case PlayerState.Game:
                {
                    //通知游戏逻辑层下线unit角色，并将数据存入数据库
                    var m2GRequestExitGame = (M2G_RequestExitGame)await player.Root().GetComponent<MessageLocationSenderComponent>()
                            .Get(LocationType.Unit).Call(player.UnitId,G2M_RequestExitGame.Create());
                    
                    //通知移除账号角色登陆信息
                    G2L_RemoveLoginRecord g2LRemoveLoginRecord = G2L_RemoveLoginRecord.Create();
                    g2LRemoveLoginRecord.AccountName = player.AccountName;
                    g2LRemoveLoginRecord.ServerId = player.Zone();
                    L2G_RemoveLoginRecord l2GRemoveLoginRecord=(L2G_RemoveLoginRecord)await player.Root().GetComponent<MessageSender>()
                            .Call(ET.StartSceneConfigCategory.Instance.LoginCenterConfig.ActorId, g2LRemoveLoginRecord);
                } break;
            }

            var timerComponent = player.Root().GetComponent<TimerComponent>();
            player.PlayerState = PlayerState.Disconnect;
            await player.GetComponent<PlayerSessionComponent>().RemoveLocation(LocationType.GateSession);
            await player.RemoveLocation(LocationType.Player);
            player?.Root().GetComponent<PlayerComponent>()?.Remove(player);
            player?.Dispose();
            await timerComponent.WaitAsync(300);
        }

        //下线销毁
        public static async ETTask KickPlayer(Player player)
        {
            if (player==null||player.IsDisposed)
            {
                return;
            }

            long instanceId = player.InstanceId;
            CoroutineLockComponent coroutineLockComponent = player.Root().GetComponent<CoroutineLockComponent>();
            using (await coroutineLockComponent.Wait(CoroutineLockType.LoginGate,player.AccountName.GetLongHashCode()))
            {
                if (player.IsDisposed||instanceId!=player.InstanceId)
                {
                    return;
                }

                await KickPlayerNoLock(player);
            }
        }
    }
}
