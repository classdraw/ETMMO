namespace ET.Server
{
    [EntitySystemOf(typeof(KnapsackComponent))]
    [FriendOfAttribute(typeof(ET.Server.KnapsackComponent))]
    public static partial  class KnapsackComponentSystem
    {
        [EntitySystem]
        private static void Awake(this ET.Server.KnapsackComponent self)
        {
            
        }
        
        [EntitySystem]
        private static void Destroy(this ET.Server.KnapsackComponent self)
        {
            self.ContainerInfoDic.Clear();

        }
        [EntitySystem]
        private static void Deserialize(this ET.Server.KnapsackComponent self)
        {

        }
    }
}

