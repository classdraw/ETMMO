namespace ET.Client
{
    /// <summary>
    /// 标记 GameObject 来自对象池，销毁时归还而非 Destroy。
    /// </summary>
    [ComponentOf(typeof(Unit))]
    public class ReUseComponent : Entity, IAwake<string>, IDestroy
    {
        public string PoolKey;
    }
}
