namespace ET.Server
{
    [EntitySystemOf(typeof(BulletComponent))]
    [FriendOf(typeof(BulletComponent))]
    public static partial class BulletComponentSystem
    {
        [EntitySystem]
        private static void Awake(this ET.Server.BulletComponent self,int configId)
        {
            self.ConfigId = configId;

        }
        [EntitySystem]
        private static void Destroy(this ET.Server.BulletComponent self)
        {
            self.ConfigId = 0;
        }

    }
}

