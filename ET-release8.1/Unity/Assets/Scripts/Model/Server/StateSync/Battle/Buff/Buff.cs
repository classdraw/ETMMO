using MongoDB.Bson.Serialization.Attributes;

namespace ET.Server
{
    [ChildOf(typeof(BuffTempComponent))]
    public class BuffCreateInfo:Entity,IAwake<int>,IDestroy,ISerializeToEntity
    {
        public int ConfigId;
        //有很多变量  辅助创建buff
        public long AddUnitId;//创建者UnitId
        public int AddCastId;//增加的技能id
        public int FirstLayer;//>0时替代配置的FirstAddLayer
    }

    [ChildOf(typeof(BuffComponent))]
    public class Buff:Entity,IAwake<int>,IDestroy,IDeserialize,ISerializeToEntity
    {
        public int ConfigId;//BuffConfig的id
        [BsonIgnore]
        public BuffConfig Config
        {
            get
            {
                return BuffConfigCategory.Instance.Get(this.ConfigId);
            }
        }


        public long AddUnitId;//创建者UnitId
        public int AddSkillId;//添加的技能id

        public long CreateTime;//创建时间
        public int TickTime;//间隔时间
        public long TickBeginTime;//开始时间
        public long ExpireTime;//buff过期时间
        public int Layer;//当前层数
        
        [BsonIgnore]
        public EntityRef<Unit> Owner;//作用者
        [BsonIgnore]
        public long TickTimer;//tick迭代器
        [BsonIgnore]
        public long WaitTickTimer;//等待迭代器
        [BsonIgnore]
        public long ExpireTimer;//过期迭代器
    }

    [EnableClass]
    public class BuffExtraData
    {
        public long AddUnitId;
        public int AddSkillId;
        public int TickTime;
        public long TickBeginTime;
        public int Layer;
    }
}

