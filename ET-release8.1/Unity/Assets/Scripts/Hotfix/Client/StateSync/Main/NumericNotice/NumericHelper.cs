using ET.Client;

namespace ET
{
    public static class NumericHelper
    {
        public static async ETTask UpdateNumeric(Scene rootScene)
        {
            C2M_TestNumericValue c2MTestNumericValue = C2M_TestNumericValue.Create();
            M2C_TestNumericValue m2CTestNumericValue = await rootScene.GetComponent<ClientSenderComponent>().Call(c2MTestNumericValue) as M2C_TestNumericValue;
        }
        
        //测试数值同步
        public static async ETTask AAA(Scene scene)
        {
            ClientSenderComponent clientSender = scene.Root().GetComponent<ClientSenderComponent>();
            if (clientSender == null)
            {
                return;
            }

            C2M_TestNumericValue request = C2M_TestNumericValue.Create();
            IResponse response = await clientSender.Call(request, false);
            if (response is M2C_TestNumericValue m2C)
            {
                Log.Info($"C2M_TestNumericValue Error={m2C.Error} response={m2C.response}");
            }
        }
    }
}

