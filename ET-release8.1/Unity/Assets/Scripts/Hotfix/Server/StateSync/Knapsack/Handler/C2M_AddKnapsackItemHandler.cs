namespace ET.Server
{
    [MessageHandler(SceneType.Map)]
    public class C2M_AddKnapsackItemHandler:MessageLocationHandler<Unit,C2M_AddKnapsackItem,M2C_AddKnapsackItem>
    {
        protected override async ETTask Run(Unit unit, C2M_AddKnapsackItem request, M2C_AddKnapsackItem response)
        {
            // GM/客户端只传 ConfigId+Count，格子 Index 由服务端 AddItem 自动找空位分配（0 起、可断档补位）。
            bool flag = KnapsackHelper.AddItemByConfigId(unit, request.ContainerType, request.ConfigId, request.Count);
            if (!flag)
            {
                response.Error = ErrorCode.ERR_AddKnapsackItemError;
            }
            await ETTask.CompletedTask;
        }
    }
}

