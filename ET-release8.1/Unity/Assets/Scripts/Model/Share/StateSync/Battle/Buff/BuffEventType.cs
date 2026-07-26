namespace ET
{
    public struct BuffTimeOut
    {
        public Unit Unit;
        public long BuffId;
    }

    /// <summary>
    /// 单位进入玩家视野后，补发该单位已有 Buff。
    /// </summary>
    public struct NoticeBuffsToViewer
    {
        public Unit Viewer;
        public Unit Owner;
    }
}

