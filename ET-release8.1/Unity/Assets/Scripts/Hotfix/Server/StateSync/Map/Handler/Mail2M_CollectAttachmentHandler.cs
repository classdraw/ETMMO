namespace ET.Server
{
    [MessageHandler(SceneType.Map)]
    public class Mail2M_CollectAttachmentHandler : MessageLocationHandler<Unit, Mail2M_CollectAttachment, M2Mail_CollectAttachment>
    {
        protected override async ETTask Run(Unit unit, Mail2M_CollectAttachment request, M2Mail_CollectAttachment response)
        {
            KnapsackContainerComponent inventoryContainer = unit.GetComponent<KnapsackComponent>().GetContainer((int)KnapsackContainerType.Inventory);
            if (inventoryContainer == null)
            {
                response.Error = ErrorCode.ERR_AddKnapsackItemError;
                await ETTask.CompletedTask;
                return;
            }

            foreach (ItemProto itemProto in request.AttachItems)
            {
                if (!inventoryContainer.CanAddItemByConfigId(itemProto.ConfigId, itemProto.Count))
                {
                    response.Error = ErrorCode.ERR_AddKnapsackItemError;
                    await ETTask.CompletedTask;
                    return;
                }
            }

            foreach (ItemProto itemProto in request.AttachItems)
            {
                if (!inventoryContainer.AddItemByConfigId(itemProto.ConfigId, itemProto.Count))
                {
                    response.Error = ErrorCode.ERR_AddKnapsackItemError;
                    await ETTask.CompletedTask;
                    return;
                }
            }
            await ETTask.CompletedTask;
        }
    }
}
