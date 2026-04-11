namespace ET.Server
{
    [Invoke((long)SceneType.UnitCache)]
    public class LRUCacheInvoker_DeleteUnitCache:AInvokeHandler<LRUUnitCacheDelete>
    {
        public override void Handle(LRUUnitCacheDelete args)
        {
            LRUCache lruCache = args.LRUCache;
            lruCache?.GetParent<UnitCacheComponent>().Delete(args.Key).Coroutine();
        }
    }
}

