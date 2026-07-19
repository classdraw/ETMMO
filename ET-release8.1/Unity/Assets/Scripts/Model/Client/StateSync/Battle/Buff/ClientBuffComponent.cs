using System.Collections.Generic;

namespace ET.Client
{
    [ComponentOf(typeof(Unit))]
    public class ClientBuffComponent:Entity,IAwake,IDestroy
    {
        public Dictionary<long, EntityRef<ClientBuff>> Buffs = new Dictionary<long, EntityRef<ClientBuff>>();
    }
}

