namespace ET.Server
{
    //unit处理逻辑的网络消息一定要是map服务器
    [MessageHandler(SceneType.Map)]
    public class C2M_TestNumericValueHandler:MessageLocationHandler<Unit,C2M_TestNumericValue,M2C_TestNumericValue>
    {
        protected override async ETTask Run(Unit unit, C2M_TestNumericValue request, M2C_TestNumericValue response)
        {
            int hp = unit.GetComponent<NumericComponent>().GetAsInt(NumericType.Hp);
            hp += 10;
            unit.GetComponent<NumericComponent>().Set(NumericType.Hp,hp);
            unit.GetComponent<NumericComponent>()[NumericType.Level] += 1;
            for (int i=0;i<100;i++) {
                unit.GetComponent<NumericComponent>()[NumericType.Level] += 1;
            }

            await ETTask.CompletedTask;
        }
    }
}

