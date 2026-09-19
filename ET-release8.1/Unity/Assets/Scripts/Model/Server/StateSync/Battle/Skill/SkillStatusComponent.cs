using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.Options;

namespace ET.Server
{
    // 技能 CD 与当前行为技能状态；附属技能（UnBreakTime==-1）只使用 CoolDowns / CoolDownStartTimes
    [ComponentOf(typeof(Unit))]
    public class SkillStatusComponent:Entity,IAwake,IDestroy,ITransfer
    {
        /// <summary>当前行为技能 Cast 实例 Id（UnBreakTime>=0）</summary>
        public long CurrentSkillCastInstanceId = default;
        /// <summary>当前行为技能配置 Id</summary>
        public long CurrentSkillCastID = default;
        /// <summary>当前行为技能开始时间（与 Cast.StartTime 一致）</summary>
        public long CurrentSkillStartTime = default;
        /// <summary>当前行为技能状态</summary>
        public SkillStatusType CurrentSkillStatus = SkillStatusType.New;
        
        [BsonDictionaryOptions(DictionaryRepresentation.ArrayOfArrays)]
        public Dictionary<int, long> CoolDowns = new Dictionary<int, long>();

        /// <summary>各技能 CD 开始时间（行为技能与附属技能均记录）</summary>
        [BsonDictionaryOptions(DictionaryRepresentation.ArrayOfArrays)]
        public Dictionary<int, long> CoolDownStartTimes = new Dictionary<int, long>();
    }
}

