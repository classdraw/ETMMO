namespace ET.Server
{
    [EntitySystemOf(typeof(ReliveComponent))]
    [FriendOf(typeof(ReliveComponent))]
    public static partial class ReliveComponentSystem
    {
        [EntitySystem]
        private static void Awake(this ET.Server.ReliveComponent self)
        {
            self.Alive = true;
        }
        [EntitySystem]
        private static void Destroy(this ET.Server.ReliveComponent self)
        {
            self.Alive = false;
        }
    }
}

