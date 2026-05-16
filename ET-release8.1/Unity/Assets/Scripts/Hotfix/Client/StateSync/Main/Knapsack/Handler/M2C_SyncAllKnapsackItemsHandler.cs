namespace ET.Client
{
    [MessageHandler(SceneType.StateSync)]
    public class M2C_SyncAllKnapsackItemsHandler: MessageHandler<Scene,M2C_SyncAllKnapsackItems>
    {
        //传送后服务器发送背包全量数据
        protected override async ETTask Run(Scene root, M2C_SyncAllKnapsackItems message)
        {
            foreach (ItemProto itemProto in message.ItemList)
            {
                root.GetComponent<ClientKnapsackComponent>().GetContainer(itemProto.ContainerType).AddItemFromMessage(itemProto);
            }

            await ETTask.CompletedTask;
        }
    }
}

