using MongoDB.Bson.Serialization.Attributes;

namespace ET
{
    [ChildOf]
    public class Item: Entity,IAwake<int>,IDestroy,ISerializeToEntity
    {
        public int ConfigId { get; set; }//itemConfig的id
        public int ContainerType { get; set; }//是哪个存储  背包还是仓库

        public int Count { get; set; }//数量
        public int Quality { get; set; }//品质

        //配置数据
        [BsonIgnore]
        private ItemConfig Config => ItemConfigCategory.Instance.Get(this.ConfigId);
    }
}

