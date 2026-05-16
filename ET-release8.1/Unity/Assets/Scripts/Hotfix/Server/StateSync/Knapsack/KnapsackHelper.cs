namespace ET.Server
{
    public static class KnapsackHelper
    {
        public static bool AddItemByConfigId(Unit unit, int configId)
        {
            KnapsackContainerComponent inventoryContainer = unit.GetComponent<KnapsackComponent>().GetContainer((int)KnapsackContainerType.Inventory);
            if ( inventoryContainer == null)
            {
                return false;
            }

            return inventoryContainer.AddItemByConfigId(configId);
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
