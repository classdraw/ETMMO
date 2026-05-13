namespace ET.Server
{
    [EntitySystemOf(typeof(KnapsackContainerComponent))]
    [FriendOfAttribute(typeof(ET.Server.KnapsackContainerComponent))]
    public static partial class KnapsackContainerComponentSystem
    {
        

        private static int GetKnapsackMaxLoad()
        {
            if (!ConstantConfigCategory.Instance.Contain(ConstantConfigKeys.ConstantKnapsackMaxLoadConfigId))
            {
                Log.Warning($"ConstantConfig 缺少 Id={ConstantConfigKeys.ConstantKnapsackMaxLoadConfigId}，背包上限使用回退值 500");
                return 200;
            }

            int v = ConstantConfigCategory.Instance.Get(ConstantConfigKeys.ConstantKnapsackMaxLoadConfigId).IntValue;
            if (v <= 0)
            {
                Log.Warning($"ConstantConfig Id={ConstantConfigKeys.ConstantKnapsackMaxLoadConfigId} IntValue<=0，背包上限使用回退值 500");
                return 200;
            }

            return v;
        }

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
            //children会序列化 但是items不会序列化，所有需要在数据库反序列化后 把children加入到items
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
        
        public static bool HasItem(this KnapsackContainerComponent self, long itemId)
        {
            self.Items.TryGetValue(itemId, out EntityRef<Item> itemRef);
            Item item = itemRef;
            return item != null && !item.IsDisposed;
        }

        public static bool IsMaxLoad(this KnapsackContainerComponent self)
        {
            return self.Items.Count >= GetKnapsackMaxLoad();
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

            ItemConfig cfg = ItemConfigCategory.Instance.Get(item.ConfigId);
            if (cfg.StackingLimit <= 0 || item.Count <= 0 || item.Count > cfg.StackingLimit)
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
        //添加直接增加
        private static bool AddItem(this KnapsackContainerComponent self,Item item)
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

            ItemConfig addCfg = ItemConfigCategory.Instance.Get(item.ConfigId);
            if (addCfg.StackingLimit <= 0 || item.Count <= 0 || item.Count > addCfg.StackingLimit)
            {
                Log.Error($"AddItem 数量非法：ConfigId={item.ConfigId}, Count={item.Count}, StackingLimit={addCfg.StackingLimit}");
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
            ItemNoticeHelper.SyncItemInfo(self.Parent.GetParent<Unit>(),item,ItemOpType.Add);
             
            //M2C_UpdateItemInfo m2CUpdateItemInfo = M2C_UpdateItemInfo.Create();
            //m2CUpdateItemInfo.Op = (int)ItemOpType.Add;
            //m2CUpdateItemInfo.ItemInfo = item.ToMessage();
            //MapMessageHelper.SendToClient(self.Parent.GetParent<Unit>(),m2CUpdateItemInfo);
             
            return true;
        }
        //-1表示直接移除该物品
        public static bool RemoveItem(this KnapsackContainerComponent self, long itemId,int count=-1)
        {
            if (!self.Items.TryGetValue(itemId, out EntityRef<Item> itemRef))
            {
                return false;
            }
            Item item = itemRef;
            if (count==-1)
            {
                self.Items.Remove(itemId);
                ItemNoticeHelper.SyncItemInfo(self.Parent.GetParent<Unit>(),item,ItemOpType.Remove);
                item?.Dispose();
            }
            else
            {
                if (item.Count<count)
                {
                    return false;//不够移除
                }
                else if(item.Count==count)//刚好移除
                {
                    self.Items.Remove(itemId);
                    ItemNoticeHelper.SyncItemInfo(self.Parent.GetParent<Unit>(),item,ItemOpType.Remove);
                    item?.Dispose();
                }
                else//只是数量改变
                {
                    item.Count -= count;
                    ItemNoticeHelper.SyncItemInfo(self.Parent.GetParent<Unit>(),item,ItemOpType.Update);
                }
                
            }
            //M2C_UpdateItemInfo m2CUpdateItemInfo = M2C_UpdateItemInfo.Create();
            //m2CUpdateItemInfo.Op = (int)ItemOpType.Remove;
            //m2CUpdateItemInfo.ItemInfo = item.ToMessage();
            //MapMessageHelper.SendToClient(self.Parent.GetParent<Unit>(),m2CUpdateItemInfo);
            
            
            return true;
        }
        
        /// <summary>
        /// 先填满同 ConfigId 的未满堆叠，再计算需要新增几格；与 <see cref="AddItemByConfigId"/> 占用规则一致。
        /// </summary>
        public static bool CanAddItemByConfigId(this KnapsackContainerComponent self, int configId, int count)
        {
            if (!ItemConfigCategory.Instance.Contain(configId) || count <= 0)
            {
                return false;
            }

            ItemConfig itemConfig = ItemConfigCategory.Instance.Get(configId);
            int stackingLimit = itemConfig.StackingLimit;
            if (stackingLimit <= 0)
            {
                return false;
            }

            int remaining = count;
            foreach (EntityRef<Item> itemRef in self.Items.Values)
            {
                Item item = itemRef;
                if (item == null || item.IsDisposed || item.ConfigId != configId)
                {
                    continue;
                }

                int space = stackingLimit - item.Count;
                if (space <= 0)
                {
                    continue;
                }

                int take = remaining < space ? remaining : space;
                remaining -= take;
                if (remaining <= 0)
                {
                    return true;
                }
            }

            int maxLoad = GetKnapsackMaxLoad();
            int newStacks = (remaining + stackingLimit - 1) / stackingLimit;
            return self.Items.Count + newStacks <= maxLoad;
        }

        //根据堆叠上限增加物品
        public static bool AddItemByConfigId(this KnapsackContainerComponent self, int configId, int count = 1)
        {
            if (!self.CanAddItemByConfigId(configId, count))//是否装得下
            {
                return false;
            }

            ItemConfig itemConfig = ItemConfigCategory.Instance.Get(configId);
            int stackingLimit = itemConfig.StackingLimit;
            int remaining = count;
            Unit unit = self.Parent.GetParent<Unit>();

            foreach (EntityRef<Item> itemRef in self.Items.Values)
            {
                Item item = itemRef;
                if (item == null || item.IsDisposed || item.ConfigId != configId)
                {
                    continue;
                }

                int space = stackingLimit - item.Count;
                //超过了上限不做处理
                if (space <= 0)
                {
                    continue;
                }

                int take = remaining < space ? remaining : space;//记录空位置
                item.Count += take;
                remaining -= take;
                ItemNoticeHelper.SyncItemInfo(unit, item, ItemOpType.Update);
                if (remaining <= 0)
                {
                    return true;
                }
            }

            while (remaining > 0)
            {
                int chunk = remaining < stackingLimit ? remaining : stackingLimit;
                Item newItem = ItemFactory.CreateItem(self, configId, chunk);
                if (newItem == null || !self.AddItem(newItem))
                {
                    Log.Error("AddItemByConfigId 创建或添加失败，与预检不一致");
                    newItem?.Dispose();
                    return false;
                }

                remaining -= chunk;
            }

            return true;
        }
    }
}

