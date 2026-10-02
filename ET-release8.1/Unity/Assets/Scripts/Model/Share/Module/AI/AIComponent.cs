namespace ET
{
    /// <summary>仅地图 Unit（怪物 / 压测 Robot），挂在 Unit 上，由服务端 AI Handler 驱动。</summary>
    [ComponentOf(typeof(Unit))]
    public class AIComponent: Entity, IAwake<int>, IDestroy, IUpdate
    {
        public int AIConfigId;

        public long Timer;

        /// <summary>当前运行的 AIConfig 节点 Id（表 key），0 表示无节点。</summary>
        public int Current;
    }
}