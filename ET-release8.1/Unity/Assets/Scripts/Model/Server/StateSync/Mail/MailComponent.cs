using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;

namespace ET.Server
{
    [ComponentOf(typeof(MailUnit))]
    public class MailComponent: Entity,IAwake,IDestroy,IDeserialize
    {
        [BsonIgnore]
        public List<EntityRef<MailInfo>> MailInfosList = new List<EntityRef<MailInfo>>();
    }
}

