namespace ET.Client
{
    [EntitySystemOf(typeof(Buff))]
    [FriendOfAttribute(typeof(ET.Client.Buff))]
    public static partial class BuffSystem
    {
        [EntitySystem]
        private static void Awake(this ET.Client.Buff self, int configId)
        {
            self.ConfigId = configId;
            self.CreateTime = TimeInfo.Instance.ServerFrameTime();
        }

        [EntitySystem]
        private static void Destroy(this ET.Client.Buff self)
        {
            self.ConfigId = 0;
            self.CreateTime = 0;
            self.ExpireTime = 0;
            self.Owner = null;
        }
    }
}

