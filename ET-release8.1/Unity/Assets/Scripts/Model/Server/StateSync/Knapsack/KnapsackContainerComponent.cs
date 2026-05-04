using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;

namespace ET.Server
{
    [ChildOf(typeof(KnapsackComponent))]
    public class KnapsackContainerComponent: Entity, IAwake<int>, IDestroy,IDeserialize,ISerializeToEntity
    {
        public int KnapsackContainerType { get; set; }//背包还是装备 还是仓库
        //这里不进行保存，是因为Entity有ChildrenCollection，里面会序列化保存
        [BsonIgnore]
        public Dictionary<long, EntityRef<Item>> Items = new Dictionary<long, EntityRef<Item>>();//所有物品
    }
}

