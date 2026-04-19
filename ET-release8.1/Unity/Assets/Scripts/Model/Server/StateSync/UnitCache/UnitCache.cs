using System.Collections.Generic;

namespace ET.Server
{
    [ChildOf(typeof(UnitCacheComponent))]
    public class UnitCache:Entity,IAwake,IDestroy
    {
        public string key;//表示什么类型数据换成 比如装备、背包
        public Dictionary<long, EntityRef<Entity>> CacheComponentDics = new Dictionary<long, EntityRef<Entity>>();
    }
}

