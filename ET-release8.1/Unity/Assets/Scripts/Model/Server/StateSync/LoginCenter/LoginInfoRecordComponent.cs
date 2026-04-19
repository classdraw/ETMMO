using System.Collections.Generic;

namespace ET.Server
{
    /// <summary>
    /// 记录角色登录数据
    /// </summary>
    [ComponentOf(typeof(Scene))]
    public class LoginInfoRecordComponent :Entity,IAwake,IDestroy
    {
        public Dictionary<long, int> AccountLoginInfoDict = new Dictionary<long, int>();
    }
}

