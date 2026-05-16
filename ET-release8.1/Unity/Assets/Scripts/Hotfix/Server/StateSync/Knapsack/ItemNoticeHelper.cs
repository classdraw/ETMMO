namespace ET.Server
{
    
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
            using (ListComponent<Item> items = ListComponent<Item>.Create())
            {
                unit.GetComponent<KnapsackComponent>().GetAllItems(items);
                foreach (Item item in items)
                {
                    m2CSyncAllKnapsackItems.ItemList.Add(item.ToMessage());
                }
            }
            MapMessageHelper.SendClient(unit,m2CSyncAllKnapsackItems,NoticeClientType.Self);
        }
    }
}
