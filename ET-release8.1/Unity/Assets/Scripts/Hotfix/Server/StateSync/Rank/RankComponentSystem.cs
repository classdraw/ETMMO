
namespace ET.Server
{   
    [EntitySystemOf(typeof(RankComponent))]
    [FriendOf(typeof(RankComponent))]
    [FriendOf(typeof(RankInfo))]
    public static partial class RankComponentSystem
    {
        [EntitySystem]
        private static void Awake(this RankComponent self)
        {

        }
        [EntitySystem]
        private static void Destroy(this RankComponent self)
        {
            foreach (RankInfo rankInfo in self.RankInfosDictionary.Values)
            {
                rankInfo?.Dispose();
            }
            self.RankInfosDictionary.Clear();
            self.SortedRankInfoList.Clear();
        }

        public static async ETTask LoadRankInfo(this RankComponent self)
        {
            var rankInfoList = await self.Root().GetComponent<DBManagerComponent>().GetZoneDB(self.Zone()).Query<RankInfo>(d => true, collection: "RankInfosComponent");

            foreach (RankInfo rankInfo in rankInfoList)
            {
                self.AddChild(rankInfo);
                self.RankInfosDictionary.Add(rankInfo.UnitId, rankInfo);
                self.SortedRankInfoList.Add(rankInfo, rankInfo.UnitId);
            }
        }
        
        public static void AddOrUpdate(this RankComponent self, RankInfo newRankInfo)
        {
            if (self.RankInfosDictionary.ContainsKey(newRankInfo.UnitId))
            {
                RankInfo oldRankInfo = self.RankInfosDictionary[newRankInfo.UnitId];
                if (oldRankInfo.RankValue == newRankInfo.RankValue)
                {
                    return;
                }

                self.Root().GetComponent<DBManagerComponent>().GetZoneDB(self.Zone()).Remove<RankInfo>(oldRankInfo.UnitId, oldRankInfo.Id, "RankInfosComponent").Coroutine();
                self.RankInfosDictionary.Remove(oldRankInfo.UnitId);
                self.SortedRankInfoList.Remove(oldRankInfo);
                oldRankInfo?.Dispose();
            }
            
            
            self.RankInfosDictionary.Add(newRankInfo.UnitId, newRankInfo);
            self.SortedRankInfoList.Add(newRankInfo, newRankInfo.UnitId);
            self.Root().GetComponent<DBManagerComponent>().GetZoneDB(self.Zone()).Save(newRankInfo.UnitId, newRankInfo, "RankInfosComponent").Coroutine();
        }
    }
}

