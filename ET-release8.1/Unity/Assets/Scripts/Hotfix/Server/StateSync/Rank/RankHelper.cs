namespace ET.Server
{
    [FriendOf(typeof(RankInfo))]
    [FriendOf(typeof(RoleInfo))]
    public static class RankHelper
    {
        public static void AddOrUpdateLevelRank(Unit unit)
        {
            using (RankInfo rankInfo = unit.Root().AddChild<RankInfo>())
            {
                
                NumericComponent numericComponent = unit.GetComponent<NumericComponent>();
                int val = 0;
                if (numericComponent!=null)
                {
                    val = numericComponent.GetAsInt(NumericType.Level);
                }

                rankInfo.UnitId = unit.Id;
                rankInfo.Name = unit.Name;
                rankInfo.RankValue = val;//等级排行榜
                Map2Rank_AddOrUpdateRankInfo message = Map2Rank_AddOrUpdateRankInfo.Create();
                message.RankInfoProto = rankInfo.ToMessage();
                ActorId rankActorId = StartSceneConfigCategory.Instance.GetBySceneName(unit.Zone(), "Rank").ActorId;
                unit.Root().GetComponent<MessageSender>().Send(rankActorId, message);
            }
        }
    }
}

