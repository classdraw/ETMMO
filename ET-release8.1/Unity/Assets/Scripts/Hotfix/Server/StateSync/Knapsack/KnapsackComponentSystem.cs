namespace ET.Server
{
    [EntitySystemOf(typeof(KnapsackComponent))]
    [FriendOfAttribute(typeof(ET.Server.KnapsackComponent))]
    public static partial  class KnapsackComponentSystem
    {
        [EntitySystem]
        private static void Awake(this ET.Server.KnapsackComponent self)
        {
            //背包
            KnapsackContainerComponent Inventory = self.AddChild<KnapsackContainerComponent, int>((int)KnapsackContainerType.Inventory);
            self.ContainerInfoDic.Add((int)KnapsackContainerType.Inventory,Inventory);
            //仓库
            KnapsackContainerComponent Warehouse = self.AddChild<KnapsackContainerComponent, int>((int)KnapsackContainerType.Warehouse);
            self.ContainerInfoDic.Add((int)KnapsackContainerType.Warehouse,Warehouse);
            //装备
            KnapsackContainerComponent Equipment = self.AddChild<KnapsackContainerComponent, int>((int)KnapsackContainerType.Equipment);
            self.ContainerInfoDic.Add((int)KnapsackContainerType.Equipment,Equipment);
        }
        
        [EntitySystem]
        private static void Destroy(this ET.Server.KnapsackComponent self)
        {
            self.ContainerInfoDic.Clear();

        }
        
        //从数据库序列化出来后 处理
        [EntitySystem]
        private static void Deserialize(this ET.Server.KnapsackComponent self)
        {
            foreach (Entity entity in self.Children.Values)
            {
                if (entity is KnapsackContainerComponent knapsackContainerComponent)
                {
                    self.ContainerInfoDic.Add(knapsackContainerComponent.KnapsackContainerType,knapsackContainerComponent);
                }
            }
        }

        public static void GetAllItems(this KnapsackComponent self,ListComponent<Item> itemList)
        {
            foreach (KnapsackContainerComponent container in self.ContainerInfoDic.Values)
            {
                container.GetItems(itemList);
            }
        }
        
        public static KnapsackContainerComponent GetContainer(this KnapsackComponent self, int containerType)
        {
            self.ContainerInfoDic.TryGetValue(containerType, out EntityRef<KnapsackContainerComponent> container);
            return container;
        }
    }
}

