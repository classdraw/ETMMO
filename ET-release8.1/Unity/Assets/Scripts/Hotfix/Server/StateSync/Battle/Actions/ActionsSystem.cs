namespace ET.Server
{
    [EntitySystemOf(typeof(Actions))]
    [FriendOf(typeof(Actions))]
    public static partial class ActionsSystem
    {
        [EntitySystem]
        private static void Awake(this ET.Server.Actions self,int configId)
        {
            self.ConfigId = configId;
        }
        [EntitySystem]
        private static void Destroy(this ET.Server.Actions self)
        {
            self.ConfigId = 0;
        }
    }
}
