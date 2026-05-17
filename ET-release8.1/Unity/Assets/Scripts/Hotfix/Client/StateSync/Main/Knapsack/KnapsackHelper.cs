namespace ET.Client
{
    public static class KnapsackHelper
    {
        public static async ETTask<int> GetAllItems(Scene root)
        {
            C2M_GetAllKnapsack c2MGetAllKnapsack = C2M_GetAllKnapsack.Create();
            M2C_GetAllKnapsack m2CGetAllKnapsack = (M2C_GetAllKnapsack)await root.GetComponent<ClientSenderComponent>().Call(c2MGetAllKnapsack);
            if (m2CGetAllKnapsack.Error!=ErrorCode.ERR_Success)
            {
                
                return m2CGetAllKnapsack.Error;
            }
            root.GetComponent<ClientKnapsackComponent>().ClearAllItems();
            Log.Info($"Knapsack GetAllItems count={m2CGetAllKnapsack.ItemList.Count}");
            foreach (var itemProto in m2CGetAllKnapsack.ItemList)
            {
                Log.Info($"Knapsack Item Id={itemProto.Id} Count={itemProto.Count} ContainerType={itemProto.ContainerType}");
                root.GetComponent<ClientKnapsackComponent>().GetContainer(itemProto.ContainerType).AddItemFromMessage(itemProto);
            }
            return ErrorCode.ERR_Success;
        }
        
        public static async ETTask<int> RequestAddItem(Scene root, KnapsackContainerType containerType, int configId)
        {
            C2M_AddKnapsackItem c2MAddKnapsackItem = C2M_AddKnapsackItem.Create();
            c2MAddKnapsackItem.ContainerType = (int)containerType;
            c2MAddKnapsackItem.ConfigId = configId;
            M2C_AddKnapsackItem m2CAddKnapsackItem = (M2C_AddKnapsackItem)await root.GetComponent<ClientSenderComponent>().Call(c2MAddKnapsackItem);
            if (m2CAddKnapsackItem.Error==ErrorCode.ERR_Success)
            {
                Log.Info($"RequestAddItem Id={configId} ContainerType={containerType} Success");
            }

            return m2CAddKnapsackItem.Error;
        }
        
        public static async ETTask<int> RequestRemoveItem(Scene root, KnapsackContainerType containerType, long itemId)
        {
            C2M_RemoveKnapsackItem c2MRemoveKnapsackItem = C2M_RemoveKnapsackItem.Create();
            c2MRemoveKnapsackItem.ContainerType = (int)containerType;
            c2MRemoveKnapsackItem.ItemId = itemId;
            M2C_RemoveKnapsackItem m2CRemoveKnapsackItem = (M2C_RemoveKnapsackItem)await root.GetComponent<ClientSenderComponent>().Call(c2MRemoveKnapsackItem);
            if (m2CRemoveKnapsackItem.Error==ErrorCode.ERR_Success)
            {
                Log.Info($"RequestRemoveItem itemId={itemId} ContainerType={containerType} Success");
            }
            
            return m2CRemoveKnapsackItem.Error;
        }
    }
}

