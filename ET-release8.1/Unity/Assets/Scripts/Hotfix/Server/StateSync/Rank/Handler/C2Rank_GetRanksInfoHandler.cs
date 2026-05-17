namespace ET.Server
{
    /// <summary>
    /// Rank 
    /// </summary>
    [MessageHandler(SceneType.Rank)]
    [FriendOfAttribute(typeof(ET.Server.RankComponent))]
    public class C2Rank_GetRanksInfoHandler : MessageHandler<Scene, C2Rank_GetRanksInfo, Rank2C_GetRanksInfo>
    {
        protected override async ETTask Run(Scene root, C2Rank_GetRanksInfo request, Rank2C_GetRanksInfo response)
        {
            RankComponent rankInfosComponent = root.GetComponent<RankComponent>();
            int count = 0;
            foreach (var rankInfo in rankInfosComponent.SortedRankInfoList)
            {
                RankInfo rank = rankInfo.Key;
                response.RankInfoProtoList.Add(rank.ToMessage());
                ++count;
                if (count >= 100)//最多100个
                {
                    break;
                }
            }
            await ETTask.CompletedTask;
        }
    }
}