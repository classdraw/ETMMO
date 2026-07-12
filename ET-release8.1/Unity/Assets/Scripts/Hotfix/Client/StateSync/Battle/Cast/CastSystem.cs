namespace ET.Client
{
    [EntitySystemOf(typeof(Cast))]
    [FriendOf(typeof(Cast))]
    public static partial class CastSystem
    {
        [EntitySystem]
        private static void Awake(this ET.Client.Cast self, int configId)
        {
            self.ConfigId = configId;
            self.CasterId = 0;
            self.TargetsId.Clear();
        }

        [EntitySystem]
        private static void Destroy(this ET.Client.Cast self)
        {
            self.ConfigId = 0;
            self.CasterId = 0;
            self.TargetsId.Clear();
        }
    }
}
