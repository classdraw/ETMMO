namespace ET.Client
{
    [EntitySystemOf(typeof(ClientCast))]
    [FriendOf(typeof(ClientCast))]
    public static partial class ClientCastSystem
    {
        [EntitySystem]
        private static void Awake(this ET.Client.ClientCast self, int configId)
        {
            self.ConfigId = configId;
            self.CasterId = 0;
            self.TargetsId.Clear();
        }

        [EntitySystem]
        private static void Destroy(this ET.Client.ClientCast self)
        {
            self.ConfigId = 0;
            self.CasterId = 0;
            self.TargetsId.Clear();
        }
    }
}
