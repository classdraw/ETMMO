using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;
using Unity.Mathematics;

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
        /// <summary>
        /// Interval 档 Tick 结算次数，配合配置 TickLimit；Tick1/Tick2 不使用。
        /// TickLimit 只停 Interval，不销毁子弹。
        /// </summary>
        public int TickCount = 0;
        public long OwnerId;
        public long InputUnitId;
        public float3 InputPos;
        [BsonIgnore]
        public long TickTimer;
        [BsonIgnore]
        public long TickTimer2;
        [BsonIgnore]
        public long TickTimer3;
        [BsonIgnore]
        public long ExpireTimer;

        [BsonIgnore]
        public List<long> Targets = new List<long>();
    }
}

