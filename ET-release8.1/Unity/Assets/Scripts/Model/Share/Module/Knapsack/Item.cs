using MongoDB.Bson.Serialization.Attributes;

namespace ET
{
    public enum ItemOpType
    {
        Add = 1,//增加物品
        Remove = 2, //移除物品
        Update = 3, //更新物品
    }

    public enum KnapsackContainerType
    {
        None = 0, //无类型
        Inventory = 1, //背包
        Warehouse = 2, //仓库
        Equipment = 3, //装备
    }
    
    [ChildOf]
    public class Item: Entity,IAwake<int>,IDestroy,ISerializeToEntity
    {
        public int ConfigId { get; set; }
        public int ContainerType { get; set; }

        public int Count { get; set; }
        public int Quality { get; set; }

        //配置数据
        [BsonIgnore]
        private ItemConfig Config => ItemConfigCategory.Instance.Get(this.ConfigId);
    }
}

