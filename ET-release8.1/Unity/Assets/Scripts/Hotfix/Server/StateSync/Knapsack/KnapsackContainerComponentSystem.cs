namespace ET.Server
{
    [EntitySystemOf(typeof(KnapsackContainerComponent))]
    [FriendOfAttribute(typeof(ET.Server.KnapsackContainerComponent))]
    public static partial class KnapsackContainerComponentSystem
    {
        [EntitySystem]
        private static void Awake(this ET.Server.KnapsackContainerComponent self, int knapsackContainerType)
        {
            self.KnapsackContainerType = knapsackContainerType;
        }
        [EntitySystem]
        private static void Destroy(this ET.Server.KnapsackContainerComponent self)
        {
            self.Items.Clear();
            self.KnapsackContainerType = (int)KnapsackContainerType.None;
        }
        [EntitySystem]
        private static void Deserialize(this ET.Server.KnapsackContainerComponent self)
        {
            foreach (Entity entity in self.Children.Values)
            {
                if (entity is Item item)
                {
                    self.Items.Add(item.Id,item);
                }
            }
        }

        public static void GetItems(this KnapsackContainerComponent self, ListComponent<Item> itemList)
        {
            foreach (var item in self.Items.Values)
            {
                itemList.Add(item);
            }
        }
        
        public static Item GetItem(this KnapsackContainerComponent self, long itemId)
        {
            self.Items.TryGetValue(itemId, out EntityRef<Item> itemRef);
            return itemRef;
        }
        
        public static bool IsItemExist(this KnapsackContainerComponent self, long itemId)
        {
            self.Items.TryGetValue(itemId, out EntityRef<Item> itemRef);
            Item item = itemRef;
            return item != null && !item.IsDisposed;
        }

        public static bool IsMaxLoad(this KnapsackContainerComponent self)
        {
            return self.Items.Count == 500;
        }

        public static bool IsCanAddItem(this KnapsackContainerComponent self,Item item)
        {
            //物品释放
            if (item==null||item.IsDisposed)
            {
                return false;
            }
            //配表不存在
            if (!ItemConfigCategory.Instance.Contain(item.ConfigId))
            {
                return false;
            }
            
            if(self.IsMaxLoad())
            {
                return false;
            }

            if (self.Items.ContainsKey(item.Id))
            {
                return false;
            }

            if (item.Parent==self)
            {
                return false;
            }

            return true;
        }

        public static bool AddItem(this KnapsackContainerComponent self,Item item)
        {
            if (item==null||item.IsDisposed)
            {
                Log.Error("item is Null!!!");
                return false;
            }
            //配表不存在
            if (!ItemConfigCategory.Instance.Contain(item.ConfigId))
            {
                Log.Error("ItemConfigCategory not Contain "+item.ConfigId+"!!!");
                return false;
            }

            if(self.IsMaxLoad())
            {
                Log.Error("bag is IsMaxLoad!");
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
            
            item.ContainerType = self.KnapsackContainerType;
            self.Items.Add(item.Id,item);

            
             
            //M2C_UpdateItemInfo m2CUpdateItemInfo = M2C_UpdateItemInfo.Create();
            //m2CUpdateItemInfo.Op = (int)ItemOpType.Add;
            //m2CUpdateItemInfo.ItemInfo = item.ToMessage();
            //MapMessageHelper.SendToClient(self.Parent.GetParent<Unit>(),m2CUpdateItemInfo);
             
            return true;
        }

        public static bool RemoveItem(this KnapsackContainerComponent self, long itemId)
        {
            if (!self.Items.TryGetValue(itemId, out EntityRef<Item> itemRef))
            {
                return false;
            }
            Item item = itemRef;
            self.Items.Remove(itemId);
            //M2C_UpdateItemInfo m2CUpdateItemInfo = M2C_UpdateItemInfo.Create();
            //m2CUpdateItemInfo.Op = (int)ItemOpType.Remove;
            //m2CUpdateItemInfo.ItemInfo = item.ToMessage();
            //MapMessageHelper.SendToClient(self.Parent.GetParent<Unit>(),m2CUpdateItemInfo);
            item?.Dispose();
            return true;
        }

        
        public static Item RemoveItemNoDispose(this KnapsackContainerComponent self, long itemId)
        {
            if (!self.Items.TryGetValue(itemId, out EntityRef<Item> itemRef))
            {
                return null;
            }

            Item item = itemRef;
            self.Items.Remove(itemId);
            //M2C_UpdateItemInfo m2CUpdateItemInfo = M2C_UpdateItemInfo.Create();
            //m2CUpdateItemInfo.Op = (int)ItemOpType.Remove;
            //m2CUpdateItemInfo.ItemInfo = item.ToMessage();
            //MapMessageHelper.SendToClient(self.Parent.GetParent<Unit>(),m2CUpdateItemInfo);
            return item;
        }
        
        
        public static bool IsCanAddItemByConfigId(this KnapsackContainerComponent self, int configID)
        {
            if (!ItemConfigCategory.Instance.Contain(configID))
            {
                return false;
            }

            if (self.IsMaxLoad())
            {
                return false;
            }
            
            return true;
        }
        
        public static bool AddItemByConfigId(this KnapsackContainerComponent self, int configId, int count = 1)
        {
            if ( !ItemConfigCategory.Instance.Contain(configId))
            {
                return false;
            }

            if ( count <= 0 )
            {
                return false;
            }

            for ( int i = 0; i < count; i++ )
            {
                Item newItem = ItemFactory.CreateItem(self, configId);
              
                if (!self.AddItem(newItem))
                {
                    Log.Error("添加物品失败！");
                    newItem?.Dispose();
                    return false;
                }
            }

            return true;
        }
    }
}

