namespace ET.Client
{
    [EntitySystemOf(typeof(NetworkCacheComponent))]
    [FriendOf(typeof(NetworkCacheComponent))]
    public static partial class NetworkCacheComponentSystem
    {
        [EntitySystem]
        private static void Awake(this NetworkCacheComponent self)
        {
            self.ClearCache();
        }

        [EntitySystem]
        private static void Destroy(this NetworkCacheComponent self)
        {
            self.ClearCache();
        }

        /// <summary>清空网络相关缓存（登出、回登录可调用）。</summary>
        public static void ClearCache(this NetworkCacheComponent self)
        {
            self.Account = string.Empty;
            self.Token = string.Empty;
            self.LastServerListResponse = null;
        }

        /// <summary>一次写入登录拉服后的账号、Token 与区服列表响应。</summary>
        public static void SetLoginServerListData(this NetworkCacheComponent self, string account, string token,
            R2C_GetServerInfos serverListResponse)
        {
            self.Account = account ?? string.Empty;
            self.Token = token ?? string.Empty;
            self.LastServerListResponse = serverListResponse;
        }
    }
}
