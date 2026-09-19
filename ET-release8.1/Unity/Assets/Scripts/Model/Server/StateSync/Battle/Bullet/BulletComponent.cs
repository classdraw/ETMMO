using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;

namespace ET.Server
{
    [ComponentOf(typeof(Unit))]
    public class BulletComponent:Entity,IAwake<int>,IDestroy
    {
        public int ConfigId=0;
        [BsonIgnore]
        public BulletConfig Config
        {
            get
            {
                return BulletConfigCategory.Instance.Get(this.ConfigId);
            }
        }
        public int TickCount = 0;
        public long OwnerId;
        [BsonIgnore]
        public long TickTimer;//迭代时间
        [BsonIgnore]
        public long TickTimer2;//迭代时间
        [BsonIgnore]
        public long TickTimer3;//迭代时间
        
        [BsonIgnore]
        public long ExpireTimer;//退出时间

        [BsonIgnore]
        public List<long> Targets = new List<long>();
    }
}

