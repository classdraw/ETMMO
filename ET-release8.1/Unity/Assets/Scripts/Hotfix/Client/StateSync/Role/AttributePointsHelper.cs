namespace ET.Client
{
    public static class AttributePointsHelper
    {
        public static async ETTask<bool> CommitAttributePoints(
            Scene root,
            int deltaStr,
            int deltaAgi,
            int deltaVit,
            int deltaInt,
            int deltaDex,
            int deltaLuk)
        {
            C2M_CommitAttributePoints request = C2M_CommitAttributePoints.Create();
            request.DeltaStr = deltaStr;
            request.DeltaAgi = deltaAgi;
            request.DeltaVit = deltaVit;
            request.DeltaInt = deltaInt;
            request.DeltaDex = deltaDex;
            request.DeltaLuk = deltaLuk;

            M2C_CommitAttributePoints response =
                await root.GetComponent<ClientSenderComponent>().Call(request) as M2C_CommitAttributePoints;
            if (response == null)
            {
                Log.Warning("CommitAttributePoints 无响应");
                return false;
            }

            if (response.Error == ErrorCode.ERR_Success)
            {
                Log.Info(response.Message ?? "属性点已分配");
                return true;
            }

            Log.Warning($"分配属性点失败 Error={response.Error} {response.Message}");
            return false;
        }
    }
}