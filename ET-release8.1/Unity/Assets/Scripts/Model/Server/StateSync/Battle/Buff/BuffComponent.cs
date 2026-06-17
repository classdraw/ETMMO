using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;

namespace ET.Server
{
    [ComponentOf(typeof(Unit))]
    public class BuffComponent:Entity,IAwake,IDestroy,IDeserialize,ITransfer
    {
        [BsonIgnore]
        public Dictionary<long, EntityRef<Buff>> BuffsDict = new Dictionary<long, EntityRef<Buff>>();
    }
}

