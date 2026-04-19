namespace ET.Server
{
    
    public enum NoticeClientType
    {
        NoNotice = 0, //不通知
        Self = 1, //仅通知自己
        Broadcast = 2, //广播aoi
        BroadcastWithoutSelf = 3, //广播aoi，除自己以外
    }
}
