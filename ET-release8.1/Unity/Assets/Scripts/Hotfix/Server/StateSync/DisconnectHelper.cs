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

        public static async ETTask KickPlayerNoLock(Player player)
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
                    (int getTeamIdError, long teamId) = await RelationshipHelper.GetUnitTeamId(player.Root(), player.UnitId);
                    if (getTeamIdError != ErrorCode.ERR_Success)
                    {
                        Log.Error($"获取 Unit TeamId 时发生错误 : {getTeamIdError}");
                    }

                    //通知游戏逻辑层下线unit角色，并将数据存入数据库
                    var m2GRequestExitGame = (M2G_RequestExitGame)await player.Root().GetComponent<MessageLocationSenderComponent>()
                            .Get(LocationType.Unit).Call(player.UnitId,G2M_RequestExitGame.Create());
                    if (m2GRequestExitGame.Error != ErrorCode.ERR_Success)
                    {
                        Log.Error($"离开Map游戏逻辑服时发生错误 : {m2GRequestExitGame.Error}");
                    }
                    
                    //通知邮件服下线MailUnit
                    Mail2G_ExitMailServer mail2GExitMailServer = (Mail2G_ExitMailServer)await player.Root().GetComponent<MessageLocationSenderComponent>()
                            .Get(LocationType.Mail).Call(player.UnitId,  G2Mail_ExitMailServer.Create());
                    if (mail2GExitMailServer.Error != ErrorCode.ERR_Success)
                    {
                        Log.Error($"离开邮件中心服时发生错误 : {mail2GExitMailServer.Error}");
                    }
                    player.Root()?.GetComponent<MessageLocationSenderComponent>()?.Get(LocationType.Mail)?.Remove(player.UnitId);

                    int exitRelationshipError = await RelationshipHelper.ExitRelationshipServer(player.Root(), player.UnitId, teamId);
                    if (exitRelationshipError != ErrorCode.ERR_Success)
                    {
                        Log.Error($"离开 Relationship 服时发生错误 : {exitRelationshipError}");
                    }
                    if (teamId > 0)
                    {
                        player.Root()?.GetComponent<MessageLocationSenderComponent>()?.Get(LocationType.Team)?.Remove(player.UnitId);
                    }
                    /**
                     *                    player.Root()?.GetComponent<MessageLocationSenderComponent>()?.Get(LocationType.Trade)?.Remove(player.UnitId);
                    
                    //通知聊天服下线ChatUnit
                    Chat2G_RequestExitChat chat2GRequestExitChat = (Chat2G_RequestExitChat)await player.Root().GetComponent<MessageLocationSenderComponent>()
                            .Get(LocationType.Chat).Call(player.UnitId,  G2Chat_RequestExitChat.Create());
                    if (chat2GRequestExitChat.Error != ErrorCode.ERR_Success)
                    {
                        Log.Error($"离开聊天中心服时发生错误 : {chat2GRequestExitChat.Error}");
                    }
                    player.Root()?.GetComponent<MessageLocationSenderComponent>()?.Get(LocationType.Chat)?.Remove(player.UnitId);

                     * 
                     */
                    //通知移除账号角色登陆信息
                    G2L_RemoveLoginRecord g2LRemoveLoginRecord = G2L_RemoveLoginRecord.Create();
                    g2LRemoveLoginRecord.AccountName = player.AccountName;
                    g2LRemoveLoginRecord.ServerId = player.Zone();
                    L2G_RemoveLoginRecord l2GRemoveLoginRecord=(L2G_RemoveLoginRecord)await player.Root().GetComponent<MessageSender>()
                            .Call(ET.StartSceneConfigCategory.Instance.LoginCenterConfig.ActorId, g2LRemoveLoginRecord);
                    if (l2GRemoveLoginRecord.Error != ErrorCode.ERR_Success)
                    {
                        Log.Error($"通知登陆中心服时发生错误 : {l2GRemoveLoginRecord.Error}");
                    }
                } break;
            }

            var timerComponent = player.Root().GetComponent<TimerComponent>();
            player.PlayerState = PlayerState.Disconnect;
            await player.GetComponent<PlayerSessionComponent>().RemoveLocation(LocationType.GateSession);
            await player.RemoveLocation(LocationType.Player);
            
            player?.Root()?.GetComponent<MessageLocationSenderComponent>()?.Get(LocationType.GateSession)?.Remove(player.UnitId);
            player?.Root()?.GetComponent<MessageLocationSenderComponent>()?.Get(LocationType.Unit)?.Remove(player.UnitId);

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
