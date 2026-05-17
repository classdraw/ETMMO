namespace ET.Server
{
    [MessageHandler(SceneType.Rank)]
    public class Map2Rank_AddOrUpdateRankInfoHandler : MessageHandler<Scene,Map2Rank_AddOrUpdateRankInfo>
    {
        protected override async ETTask Run(Scene scene, Map2Rank_AddOrUpdateRankInfo message)
        {
            RankComponent rankComponent = scene.GetComponent<RankComponent>();
            var rankInfo = rankComponent.AddChildWithId<RankInfo>(message.RankInfoProto.Id);
            rankInfo.FromMessage(message.RankInfoProto);
            rankComponent.AddOrUpdate(rankInfo);
            await ETTask.CompletedTask;
        }
    }
}