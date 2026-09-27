using System.Collections.Generic;

namespace ET.Client
{
    [ComponentOf(typeof(Unit))]
    public class ClientSkillStatusComponent : Entity, IAwake, IDestroy
    {
        /// <summary>与服务端 SkillStatusComponent.CurrentSkill 对应，仅行为技能（UnBreakTime&gt;=0）。</summary>
        public long CurrentSkillCastInstanceId;

        public Dictionary<int, long> CoolDownEndTimes = new Dictionary<int, long>();
        public Dictionary<int, long> CoolDownStartTimes = new Dictionary<int, long>();
    }
}
