using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.Options;

namespace ET.Server
{
    //技能状态相关 比如CD
    [ComponentOf(typeof(Unit))]
    public class SkillStatusComponent:Entity,IAwake,IDestroy,ITransfer
    {
        public long CurrentSkillCastInstanceId = default;//当前技能id
        public long CurrentSkillCastID = default;

        public long CurrentSkillStartTime = default;//当前技能开始时间
        
        public SkillStatusType CurrentSkillStatus = SkillStatusType.New;//当前技能状态
        
        [BsonDictionaryOptions(DictionaryRepresentation.ArrayOfArrays)]
        public Dictionary<int, long> CoolDowns = new Dictionary<int, long>();
    }
}

