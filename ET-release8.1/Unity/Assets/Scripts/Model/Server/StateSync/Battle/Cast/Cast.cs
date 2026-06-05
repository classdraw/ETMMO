using System.Collections.Generic;
using System.Numerics;
using MongoDB.Bson.Serialization.Attributes;

namespace ET.Server
{
    [ChildOf(typeof(CastComponent))]
    public class Cast:Entity,IAwake<int>,IDestroy
    {
        public int ConfigId;//cast表id

        [BsonIgnore]
        public CastConfig Config
        {
            get
            {
                return CastConfigCategory.Instance.Get(this.ConfigId);
            }
        }
        
        [BsonIgnore]
        public EntityRef<Unit> Caster;//释放者
        [BsonIgnore]
        public List<long> Targets = new List<long>();//技能目标


        public long inputUnitId;//输入的id
        public Vector3 inputPos;
        public long StartTime;//技能开始时间
    }
}

