using System;

namespace ET.Client
{
    public static class RankHelper
    {
        public static async ETTask<int> GetRankInfo(Scene root)
        {
            Rank2C_GetRanksInfo rank2CGetRanksInfo = null;
            try
            {
                C2Rank_GetRanksInfo c2RankGetRanksInfo = C2Rank_GetRanksInfo.Create();
                rank2CGetRanksInfo = (Rank2C_GetRanksInfo) await root.GetComponent<ClientSenderComponent>().Call(c2RankGetRanksInfo);
            }
            catch (Exception e)
            {
                Log.Error(e.ToString());
                return ErrorCode.ERR_NetWorkError;
            }
            if (rank2CGetRanksInfo.Error != ErrorCode.ERR_Success)
            {
                return rank2CGetRanksInfo.Error;
            }
            Log.Console(rank2CGetRanksInfo.RankInfoProtoList.Count+"");
            return rank2CGetRanksInfo.Error;
        }
    }
}

