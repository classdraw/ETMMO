using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;

namespace ET.Server
{
    /// <summary>
    /// 队伍持久化数据（ComponentOf TeamUnit，DB Key = TeamId）
    /// </summary>
    [ComponentOf(typeof(TeamUnit))]
    public class TeamComponent : Entity, IAwake, IDestroy,IDeserialize
    {
        public string Name;

        /// <summary>
        /// 队伍内所有成员的数据库 UnitId
        /// </summary>
        public List<long> MemberUnitIds = new();

        public long LeaderUnitId;//队长

        /// <summary>
        /// 当前在线成员 UnitId
        /// </summary>
        [BsonIgnore]
        public HashSet<long> OnlineUnitIds = new();
    }
}
