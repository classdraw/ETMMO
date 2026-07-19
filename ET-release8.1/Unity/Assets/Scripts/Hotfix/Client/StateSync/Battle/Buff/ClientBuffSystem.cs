namespace ET.Client
{
    [EntitySystemOf(typeof(ClientBuff))]
    [FriendOfAttribute(typeof(ET.Client.ClientBuff))]
    public static partial class ClientBuffSystem
    {
        [EntitySystem]
        private static void Awake(this ET.Client.ClientBuff self, int configId)
        {
            self.ConfigId = configId;
            self.CreateTime = TimeInfo.Instance.ServerFrameTime();
            self.ExpireTime = 0;
            self.Owner = null;
        }

        [EntitySystem]
        private static void Destroy(this ET.Client.ClientBuff self)
        {
            self.ConfigId = 0;
            self.CreateTime = 0;
            self.ExpireTime = 0;
            self.Owner = null;
        }
    }
}

