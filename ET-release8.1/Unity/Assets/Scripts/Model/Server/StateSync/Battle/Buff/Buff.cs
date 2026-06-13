using MongoDB.Bson.Serialization.Attributes;

namespace ET.Server
{
    [ChildOf(typeof(BuffTempComponent))]
    public class BuffCreateInfo:Entity,IAwake<int>,IDestroy,ISerializeToEntity
    {
        public int ConfigId;
        //有很多变量  辅助创建buff
    }

    [ChildOf(typeof(BuffComponent))]
    public class Buff:Entity,IAwake<int>,IDestroy,ISerializeToEntity
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
        [BsonIgnore]
        public EntityRef<Unit> Owner;//释放者

        public long CreateTime;//创建时间
        public int TickTime;//间隔时间
        public long TickBeginTime;//开始时间
        public long ExpireTime;//buff过期时间
        
        [BsonIgnore]
        public long TickTimer;//tick迭代器
        [BsonIgnore]
        public long WaitTickTimer;//等待迭代器
        [BsonIgnore]
        public long ExpireTimer;//过期迭代器
    }
}

