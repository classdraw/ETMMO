using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;

namespace ET.Server
{
    [ComponentOf(typeof(Scene))]
    public class RankComponent:Entity,IAwake,IDestroy
    {
        [BsonIgnore]
        public SortedList<EntityRef<RankInfo>, long> SortedRankInfoList  = new SortedList<EntityRef<RankInfo>, long>(new RankInfoCompare());
        
        [BsonIgnore]
        public Dictionary<long, EntityRef<RankInfo>> RankInfosDictionary = new Dictionary<long, EntityRef<RankInfo>>();

    }
    
    [EnableClass]
    [FriendOfAttribute(typeof(RankInfo))]
    public class RankInfoCompare : IComparer<EntityRef<RankInfo>>
    {
        public int Compare(EntityRef<RankInfo> a, EntityRef<RankInfo> b)
        {
            RankInfo aRankInfo = a;
            RankInfo bRankInfo = b;
            var result = aRankInfo.RankValue - bRankInfo.RankValue;

            if (result != 0)
            {
                return (int)result;
            }

            if (aRankInfo.Id < bRankInfo.Id)
            {
                return 1;
            }

            if (aRankInfo.Id > bRankInfo.Id)
            {
                return -1;
            }
            return 0;
        }
    }
}

