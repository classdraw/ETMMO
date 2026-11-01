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

        /// <summary>背包容器内格子下标，从 0 开始，范围 0 ~ 上限-1；未分配为 -1。</summary>
        public int Index { get; set; } = -1;

        //配置数据 不会存入数据库
        [BsonIgnore]
        public ItemConfig Config => ItemConfigCategory.Instance.Get(this.ConfigId);
    }
}

