namespace ET.Server
{
    public static class KnapsackHelper
    {
        public static bool AddItemByConfigId(Unit unit, int containerType, int configId, int count)
        {
            if (count <= 0)
            {
                count = 1;
            }

            KnapsackContainerComponent container = unit.GetComponent<KnapsackComponent>().GetContainer(containerType);
            if (container == null)
            {
                return false;
            }

            return container.AddItemByConfigId(configId, count);
        }
        
        public static bool RemoveItemById(Unit unit, long itemId)
        {
            KnapsackContainerComponent inventoryContainer = unit.GetComponent<KnapsackComponent>().GetContainer((int)KnapsackContainerType.Inventory);
            if ( inventoryContainer == null)
            {
                return false;
            }

            return inventoryContainer.RemoveItem(itemId);
        }

    }
}
