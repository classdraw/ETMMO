namespace ET
{
    /// <summary>
    /// 区服/分区类型，前后端共用。
    /// </summary>
    public enum ZoneType : int
    {
        /// <summary>普通区</summary>
        Normal = 1,

        /// <summary>机器人服</summary>
        Robot = 2,

        /// <summary>跨服游戏区</summary>
        CrossServer = 3,

        /// <summary>全服共享</summary>
        GlobalShared = 1000,
    }
}
