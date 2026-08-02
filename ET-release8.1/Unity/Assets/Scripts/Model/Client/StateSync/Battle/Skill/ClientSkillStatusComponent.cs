using System.Collections.Generic;

namespace ET.Client
{
    [ComponentOf(typeof(Unit))]
    public class ClientSkillStatusComponent : Entity, IAwake, IDestroy
    {
        public Dictionary<int, long> CoolDownEndTimes = new Dictionary<int, long>();
        public Dictionary<int, long> CoolDownStartTimes = new Dictionary<int, long>();
    }
}
