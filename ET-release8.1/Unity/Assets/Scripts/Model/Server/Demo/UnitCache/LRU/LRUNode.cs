namespace ET.Server
{
    [ChildOf(typeof(LRUCache))]
    public class LRUNode : Entity, IAwake<long>,IDestroy
    {
        public long Key;//一般是unitId
        public int Frequency;//越小使用次数越少 越会被移除
    }
}

