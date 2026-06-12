namespace ET.Server
{
    [EntitySystemOf(typeof(ActionsTempComponent))]
    [FriendOf(typeof(ActionsTempComponent))]
    [FriendOf(typeof(Actions))]
    public static partial class ActionsTempComponentSystem
    {
        [EntitySystem]
        private static void Awake(this ET.Server.ActionsTempComponent self)
        {
            
        }
        [EntitySystem]
        private static void Destroy(this ET.Server.ActionsTempComponent self)
        {

        }
        

    }
}

