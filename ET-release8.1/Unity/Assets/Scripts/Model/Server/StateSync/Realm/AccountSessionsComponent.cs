using System.Collections.Generic;

namespace ET.Server
{
    /// <summary>
    /// 管理账号和session连接映射关系
    /// </summary>
    [ComponentOf(typeof(Scene))]
    public class AccountSessionsComponent :Entity,IAwake,IDestroy
    {
        public Dictionary<string, EntityRef<Session>> AccountSessionDictionary = new Dictionary<string, EntityRef<Session>>();
    }
    
}

