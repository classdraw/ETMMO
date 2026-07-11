using System.Collections.Generic;

namespace ET.Client
{
    /// <summary>
    /// 客户端的cast
    /// </summary>
    [ChildOf(typeof(CastComponent))]
    public class Cast:Entity,IAwake<int>,IDestroy
    {
        public int ConfigId;
        public CastConfig Config => CastConfigCategory.Instance.Get(this.ConfigId);

        public long CasterId;

        public List<long> TargetsId = new List<long>();
    }
}

