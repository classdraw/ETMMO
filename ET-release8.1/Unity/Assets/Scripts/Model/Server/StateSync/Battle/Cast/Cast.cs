using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;
using Unity.Mathematics;

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
        public EntityRef<Unit> InputUnit;//释放者
        
        [BsonIgnore]
        public List<long> Targets = new List<long>();//技能目标


        public long InputUnitId;//输入的id
        public float3 InputPos;
        public long StartTime;//技能开始时间
    }
}

