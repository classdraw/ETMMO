using System.Collections.Generic;

namespace ET.Client
{
    [ComponentOf(typeof(Unit))]
    public class CastComponent:Entity,IAwake,IDestroy
    {
        public Dictionary<long, EntityRef<Cast>> Casts = new Dictionary<long, EntityRef<Cast>>();
    }
}

