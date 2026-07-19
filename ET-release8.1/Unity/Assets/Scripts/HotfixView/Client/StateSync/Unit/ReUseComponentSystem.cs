namespace ET.Client
{
    [EntitySystemOf(typeof(ReUseComponent))]
    [FriendOf(typeof(ReUseComponent))]
    public static partial class ReUseComponentSystem
    {
        [EntitySystem]
        private static void Awake(this ReUseComponent self, string poolKey)
        {
            self.PoolKey = poolKey;
        }

        [EntitySystem]
        private static void Destroy(this ReUseComponent self)
        {
            self.PoolKey = null;
        }
    }
}
