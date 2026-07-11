using System.Collections.Generic;

namespace ET.Client
{
    [ComponentOf(typeof(Unit))]
    public class BuffComponent:Entity,IAwake,IDestroy
    {
        public Dictionary<long, EntityRef<Buff>> Buffs = new Dictionary<long, EntityRef<Buff>>();
    }
}

