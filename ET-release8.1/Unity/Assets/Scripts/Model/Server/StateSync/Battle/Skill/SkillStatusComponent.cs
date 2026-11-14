using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.Options;

namespace ET.Server
{
    // 玩家/怪物/Robot；子弹无。IUnitCache 随玩家 UnitDBSave 入库。
    [ComponentOf(typeof(Unit))]
    public class SkillStatusComponent: Entity, IAwake, IDestroy, ITransfer, IUnitCache
    {
        /// <summary>已学技能等级：CastConfig.Id → 等级（1 起）；无条目表示未学习。</summary>
        [BsonDictionaryOptions(DictionaryRepresentation.ArrayOfArrays)]
        public Dictionary<int, int> SkillLevels = new Dictionary<int, int>();

        /// <summary>当前行为技能 Cast 实例 Id（UnBreakTime>=0）</summary>
        [BsonIgnore]
        public long CurrentSkillCastInstanceId = default;
        /// <summary>当前行为技能配置 Id</summary>
        [BsonIgnore]
        public long CurrentSkillCastID = default;
        /// <summary>当前行为技能开始时间（与 Cast.StartTime 一致）</summary>
        [BsonIgnore]
        public long CurrentSkillStartTime = default;
        /// <summary>当前行为技能状态</summary>
        [BsonIgnore]
        public SkillStatusType CurrentSkillStatus = SkillStatusType.New;
        
        /// <summary>CastConfig.Id → CD 结束时间。</summary>
        [BsonIgnore]
        [BsonDictionaryOptions(DictionaryRepresentation.ArrayOfArrays)]
        public Dictionary<int, long> CoolDowns = new Dictionary<int, long>();

        /// <summary>CastConfig.Id → CD 开始时间。</summary>
        [BsonIgnore]
        [BsonDictionaryOptions(DictionaryRepresentation.ArrayOfArrays)]
        public Dictionary<int, long> CoolDownStartTimes = new Dictionary<int, long>();
    }
}

