namespace ET.Server
{
    [EntitySystemOf(typeof(ServerInfoManagerComponent))]
    [FriendOf(typeof(ServerInfo))]
    [FriendOf(typeof(ServerInfoManagerComponent))]
    public static partial class ServerInfoManagerComponentSystem
    {
        [EntitySystem]
        private static void Awake(this ServerInfoManagerComponent self)
        {
            self.LoadAllServerInfos();
        }
        
        [EntitySystem]
        private static void Destroy(this ServerInfoManagerComponent self)
        {
            self.ClearAllServerInfos();
        }

        public static void LoadAllServerInfos(this ServerInfoManagerComponent self)
        {
            self.ClearAllServerInfos();

            var serverInfoConfigs = StartZoneConfigCategory.Instance.GetAll();
            foreach (var info in serverInfoConfigs.Values)
            {
                if (info.ZoneType!=1)//1表示可选区服
                {
                    continue;
                }

                ServerInfo newServerInfo = self.AddChildWithId<ServerInfo>(info.Id);
                newServerInfo.ServerName = info.DBName;
                newServerInfo.Status = (int)ServerStatus.Normal;
                self.ServerInfos.Add(newServerInfo);
            }
            
        }

        public static void ClearAllServerInfos(this ServerInfoManagerComponent self)
        {
            foreach (var serverInfoRef in self.ServerInfos)
            {
                ServerInfo serverInfo = serverInfoRef;
                serverInfo?.Dispose();
            }
            self.ServerInfos.Clear();
        }
    }
    
}

