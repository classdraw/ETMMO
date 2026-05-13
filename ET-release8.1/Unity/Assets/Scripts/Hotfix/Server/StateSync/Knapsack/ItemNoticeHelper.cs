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
    }
}
