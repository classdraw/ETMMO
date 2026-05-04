using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;

namespace ET.Server
{
    [ComponentOf(typeof(Unit))]
    public class KnapsackComponent: Entity,IAwake,IDestroy,IDeserialize,IUnitCache
    {
        //一个通用容器组件下面有一个子容器，每个子容器管理一堆item
        [BsonIgnore]
        public Dictionary<int, EntityRef<KnapsackContainerComponent>> ContainerInfoDic = new Dictionary<int, EntityRef<KnapsackContainerComponent>>();
    }
}

