namespace ET.Client
{
    [EntitySystemOf(typeof(ClientKnapsackComponent))]
    [FriendOfAttribute(typeof(ET.Client.ClientKnapsackComponent))]
    public static partial class ClientKnapsackComponentSystem
    {
        [EntitySystem]
        private static void Awake(this ET.Client.ClientKnapsackComponent self)
        {
            ClientKnapsackContainerComponent Inventory = self.AddChild<ClientKnapsackContainerComponent, int>((int)KnapsackContainerType.Inventory);
            self.ContainerInfoDic.Add((int)KnapsackContainerType.Inventory, Inventory);

            ClientKnapsackContainerComponent Warehouse = self.AddChild<ClientKnapsackContainerComponent, int>((int)KnapsackContainerType.Warehouse);
            self.ContainerInfoDic.Add((int)KnapsackContainerType.Warehouse, Warehouse);
            
            ClientKnapsackContainerComponent Equipment = self.AddChild<ClientKnapsackContainerComponent, int>((int)KnapsackContainerType.Equipment);
            self.ContainerInfoDic.Add((int)KnapsackContainerType.Equipment, Equipment);
        }
        [EntitySystem]
        private static void Destroy(this ET.Client.ClientKnapsackComponent self)
        {
            self.ContainerInfoDic.Clear();
        }
        
        public static ClientKnapsackContainerComponent GetContainer(this ClientKnapsackComponent self, int containerType)
        {
            if (!self.ContainerInfoDic.TryGetValue(containerType, out EntityRef<ClientKnapsackContainerComponent> container))
            {
                Log.Error($"Container not found :{(KnapsackContainerType)containerType}");
            }
            return container;
        }
        
        public static void ClearAllItems(this ClientKnapsackComponent self)
        {
            foreach (var entityRef in self.ContainerInfoDic.Values)
            {
                ClientKnapsackContainerComponent container = entityRef;
                container.Clear();
            }
        }

        /// <summary>与服务器 KnapsackComponent.GetAllItems 一致，汇总各容器物品。</summary>
        public static void GetAllItems(this ClientKnapsackComponent self, ListComponent<Item> itemList)
        {
            foreach (EntityRef<ClientKnapsackContainerComponent> containerRef in self.ContainerInfoDic.Values)
            {
                ClientKnapsackContainerComponent container = containerRef;
                container?.GetItems(itemList);
            }
        }

        public static void GetInventoryItems(this ClientKnapsackComponent self, ListComponent<Item> itemList)
        {
            ClientKnapsackContainerComponent inventory = self.GetContainer((int)KnapsackContainerType.Inventory);
            inventory?.GetItems(itemList);
        }
    }
}

