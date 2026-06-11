using MongoDB.Bson.Serialization.Attributes;

namespace ET.Server
{
    [ChildOf(typeof(ActionsTempComponent))]
    public class Actions:Entity,IAwake<int>,IDestroy,ISerializeToEntity
    {
        public int ConfigId;//ActionsConfig配表id
        [BsonIgnore]
        public ActionsConfig Config
        {
            get
            {
                return ActionsConfigCategory.Instance.Get(this.ConfigId);
            }
        }
        [BsonIgnore]
        public EntityRef<Unit> Caster;//释放者
        [BsonIgnore]
        public EntityRef<Unit> Owner;//作用者
        [BsonIgnore]
        public Cast CastSelf
        {
            get
            {
                return this.Parent.GetParent<Cast>();
            }
        }
        [BsonIgnore]
        public Buff BuffSelf
        {
            get
            {
                return this.Parent.GetParent<Buff>();
            }
        }
    }
}

