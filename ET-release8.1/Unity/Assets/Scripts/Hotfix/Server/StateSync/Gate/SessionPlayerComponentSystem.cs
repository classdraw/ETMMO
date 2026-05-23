namespace ET.Server
{
    [EntitySystemOf(typeof(SessionPlayerComponent))]
    [FriendOf(typeof(PlayerSessionComponent))]
    public static partial class SessionPlayerComponentSystem
    {
        /// <summary>
        /// Session 异常断开时启动离线倒计时；已重连或已有计时组件则跳过。
        /// 服务器主动断线应先 RemoveComponent&lt;SessionPlayerComponent&gt; 再 Dispose Session，并自行调用本方法。
        /// </summary>
        public static void TryStartPlayerOfflineOutTime(Player player)
        {
            if (player == null || player.IsDisposed)
            {
                return;
            }

            PlayerSessionComponent playerSessionComponent = player.GetComponent<PlayerSessionComponent>();
            Session current = playerSessionComponent?.Session;
            if (current != null && !current.IsDisposed)
            {
                return;
            }

            if (playerSessionComponent != null)
            {
                playerSessionComponent.Session = null;
            }

            if (player.GetComponent<PlayerOfflineOutTimeComponent>() != null)
            {
                return;
            }

            Log.Console("Session断开,进入OfflineOutTime流程");
            player.AddComponent<PlayerOfflineOutTimeComponent>();
        }

        [EntitySystem]
        private static void Destroy(this SessionPlayerComponent self)
        {
            Scene root = self.Root();
            if (root.IsDisposed)
            {
                return;
            }
            
            Player player = self.Player;
            // 如果是服务器主动跟客户端断开，要先移除 SessionPlayerComponent，再销毁 Session，否则视为突然断开
            if (player != null && self.GetParent<Session>().IsDisposed)
            {
                TryStartPlayerOfflineOutTime(player);
            }

            self.Player = null;
            
            // 这里不处理了 由DisconnectHelper处理
            //root.GetComponent<MessageLocationSenderComponent>().Get(LocationType.Unit).Send(self.Player.Id, G2M_SessionDisconnect.Create());
        }
        
        [EntitySystem]
        private static void Awake(this SessionPlayerComponent self)
        {

        }
    }
}