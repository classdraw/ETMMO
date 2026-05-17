namespace ET.Server
{
    [FriendOf(typeof(KnapsackComponent))]
    [FriendOf(typeof(KnapsackContainerComponent))]
    public static class ItemNoticeHelper
    {
        //同步物品信息
        public static void SyncItemInfo(Unit unit ,Item item,ItemOpType itemOpType)
        {
            M2C_UpdateItemInfo m2CUpdateItemInfo = M2C_UpdateItemInfo.Create();
            m2CUpdateItemInfo.Op = (int)itemOpType;
            m2CUpdateItemInfo.ItemInfo = item.ToMessage();
            MapMessageHelper.SendClient(unit,m2CUpdateItemInfo,NoticeClientType.Self);
        }
        
        public static void SyncAllKnapsackItems(Unit unit)
        {
            M2C_SyncAllKnapsackItems m2CSyncAllKnapsackItems = M2C_SyncAllKnapsackItems.Create();
            KnapsackComponent knapsack = unit.GetComponent<KnapsackComponent>();
            if (knapsack != null)
            {
                foreach (KnapsackContainerComponent container in knapsack.ContainerInfoDic.Values)
                {
                    if (container == null)
                    {
                        continue;
                    }

                    foreach (EntityRef<Item> itemRef in container.Items.Values)
                    {
                        Item item = itemRef;
                        if (item == null || item.IsDisposed)
                        {
                            continue;
                        }

                        m2CSyncAllKnapsackItems.ItemList.Add(item.ToMessage());
                    }
                }
            }

            MapMessageHelper.SendClient(unit,m2CSyncAllKnapsackItems,NoticeClientType.Self);
        }
    }
}
