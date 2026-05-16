namespace ET.Client
{
    [EntitySystemOf(typeof(ClientKnapsackContainerComponent))]
    [FriendOf(typeof(ClientKnapsackContainerComponent))]
    public static partial class ClientKnapsackContainerComponentSystem
    {
        [EntitySystem]
        private static void Awake(this ET.Client.ClientKnapsackContainerComponent self, int args2)
        {

        }
        [EntitySystem]
        private static void Destroy(this ET.Client.ClientKnapsackContainerComponent self)
        {

        }
        //增加item
        public static void AddItemFromMessage(this ClientKnapsackContainerComponent self, ItemProto itemProto)
        {
            Item item = self.AddChildWithId<Item, int>(itemProto.Id, itemProto.ConfigId);
            item.FromMessage(itemProto);
            self.Items.Add(item.Id, item);
        }
        
        //移除item
        public static void RemoveItemById(this ClientKnapsackContainerComponent self, long itemId)
        {
            if (!self.Items.TryGetValue(itemId, out var itemRef))
            {
                Log.Error($"itemid:{itemId} not found");
                return;
            }

            Item item = itemRef;
            self.Items.Remove(itemId);
            item?.Dispose();
        }
        
        public static void UpdateItem(this ClientKnapsackContainerComponent self, ItemProto itemProto)
        {
            if (!self.Items.TryGetValue(itemProto.Id, out var itemRef))
            {
                Log.Error($"itemid:{itemProto.Id} not found");
                return;
            }

            Item item = itemRef;
            item.FromMessage(itemProto);
        }
        
        public static void Clear(this ClientKnapsackContainerComponent self)
        {
            foreach (var itemRef in self.Items.Values)
            {
                Item item = itemRef;
                item?.Dispose();
            }
            self.Items.Clear();
        }

        
        public static Item GetItem(this ClientKnapsackContainerComponent self, long itemId)
        {
            self.Items.TryGetValue(itemId, out EntityRef<Item> itemRef);
            return itemRef;
        }
        
        public static bool AddItem(this ClientKnapsackContainerComponent self, Item item)
        {
            if (item == null || item.IsDisposed)
            {
                Log.Error("item is null!");
                return false;
            }
            
            if (item.Parent != self)
            {
                self.AddChild(item);
            }

            if (self.Items.ContainsKey(item.Id))
            {
                return false;
            }
            
            self.Items.Add(item.Id,item);
            return true;
        }
        
        public static void RemoveItem(this ClientKnapsackContainerComponent self, Item item)
        {
            if (item == null)
            {
                Log.Error("bag item is null");
                return; 
            }
            
            self.RemoveItemById(item.Id);
        }
    }
}

