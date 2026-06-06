namespace ET.Server
{
    [EntitySystemOf(typeof (MapComponent))]
    [FriendOf(typeof(MapComponent))]
    public static partial class MapComponentSystem
    {
        [EntitySystem]
        private static void Awake(this MapComponent self)
        {

        }
        
        [EntitySystem]
        private static void Destroy(this MapComponent self)
        {
            
        }
        
        public static async ETTask InitMap(this MapComponent self, M2M_InitMap message)
        {
            self.MapConfigId = message.MapConfigId;
            self.ctx = message.Ctx;
            await ETTask.CompletedTask;
        }
    }
}

