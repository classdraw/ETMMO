using System;

namespace ET.Server
{
    public static partial class ItemFactory
    {
        public static Item CreateItem(KnapsackContainerComponent parent, int configId,int count)
        {
            if (count <= 0)
            {
                Log.Error($"CreateItem count 非法：{count}");
                return null;
            }

            if (!ItemConfigCategory.Instance.Contain(configId))
            {
                Log.Error($"当前所创建的物品id不存在：{configId}");
                return null;
            }

            ItemConfig cfg = ItemConfigCategory.Instance.Get(configId);
            if (cfg.StackingLimit <= 0 || count > cfg.StackingLimit)
            {
                Log.Error($"CreateItem 数量与堆叠上限不符：configId={configId}, count={count}, StackingLimit={cfg.StackingLimit}");
                return null;
            }

            Item item = parent.AddChild<Item, int>(configId);
            item.ContainerType = parent.KnapsackContainerType;
            item.Count = count;
            item.Init();
            return item;
        }
        
        public static void Init(this Item self)
        {
            switch (self.Config.Type)
            {
                case (int)ItemType.Currency:
                    break;
                case (int)ItemType.Equip:
                    //self.AddComponent<EquipInfoComponent>();
                    break;
                case (int)ItemType.Item:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}

