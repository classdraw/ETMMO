namespace ET.Client
{
    [MessageHandler(SceneType.StateSync)]
    public class M2C_UpdateItemInfoHandler:MessageHandler<Scene,M2C_UpdateItemInfo>
    {
        protected override async ETTask Run(Scene root, M2C_UpdateItemInfo message)
        {
            var clientKnapsackComponent = root.GetComponent<ClientKnapsackComponent>();
            ClientKnapsackContainerComponent container = clientKnapsackComponent.GetContainer(message.ItemInfo.ContainerType);
            if (message.Op==(int)ItemOpType.Add)
            {
                container?.AddItemFromMessage(message.ItemInfo);
            }else if (message.Op==(int)ItemOpType.Update)
            {
                container?.UpdateItem(message.ItemInfo);
            }else if (message.Op==(int)ItemOpType.Remove)
            {
                container?.RemoveItemById(message.ItemInfo.Id);
            }

            EventSystem.Instance.Publish(root, new ItemInfoChange(){ItemProto = message.ItemInfo});
            await ETTask.CompletedTask;
        }
    }
}

