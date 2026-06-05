using MemoryPack;

namespace ET
{
    /// <summary>
    /// 创建地图的上下文
    /// </summary>
    [MemoryPackable]
    public partial struct CreateMapCtx
    {
        /// <summary>
        /// 创建者id
        /// </summary>
        public long CreateId;

        /// <summary>
        /// 过期时间
        /// </summary>
        public long ExpiredTime;

        /// <summary>
        /// 额外数据(Json)
        /// </summary>
        public string Data;
    }
}

