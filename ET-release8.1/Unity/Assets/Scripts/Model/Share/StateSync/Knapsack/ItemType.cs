namespace ET
{
    //物品操作行为的枚举
    public enum ItemOpType
    {
        Add = 1,//增加物品
        Remove = 2, //移除物品
        Update = 3, //更新物品
    }
    
    //各种背包类型枚举
    public enum KnapsackContainerType
    {
        None = 0, //无类型
        Inventory = 1, //背包
        Warehouse = 2, //仓库
        Equipment = 3, //装备
    }
}

