namespace ET.Server
{
    [MessageHandler(SceneType.Map)]
    public class C2M_AddKnapsackItemHandler:MessageLocationHandler<Unit,C2M_AddKnapsackItem,M2C_AddKnapsackItem>
    {
        protected override async ETTask Run(Unit unit, C2M_AddKnapsackItem request, M2C_AddKnapsackItem response)
        {
            //KnapsackContainerComponent containerComponent = unit.GetComponent<KnapsackComponent>().GetContainer(request.ContainerType);
            bool flag=KnapsackHelper.AddItemByConfigId(unit,request.ConfigId);
            if (!flag)
            {
                response.Error = ErrorCode.ERR_AddKnapsackItemError;
            }
            await ETTask.CompletedTask;
        }
    }
}

