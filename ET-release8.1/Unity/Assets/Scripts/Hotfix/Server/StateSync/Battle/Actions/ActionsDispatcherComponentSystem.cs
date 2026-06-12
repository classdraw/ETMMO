namespace ET.Server
{
    [EntitySystemOf(typeof(ActionsDispatcherComponent))]
    [FriendOf(typeof(ActionsDispatcherComponent))]
    public static partial class ActionsDispatcherComponentSystem
    {
        [EntitySystem]
        private static void Awake(this ET.Server.ActionsDispatcherComponent self)
        {
            
        }
        [EntitySystem]
        private static void Destroy(this ET.Server.ActionsDispatcherComponent self)
        {
            self.Dict.Clear();
        }
    }
}

