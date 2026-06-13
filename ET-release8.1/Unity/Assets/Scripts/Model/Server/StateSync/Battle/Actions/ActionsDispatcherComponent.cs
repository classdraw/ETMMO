using System.Collections.Generic;

namespace ET.Server
{
    [ComponentOf(typeof(Scene))]
    public class ActionsDispatcherComponent:Entity,IAwake,IDestroy
    {
        public Dictionary<int, IActions> ActionsDict = new Dictionary<int, IActions>();
    }
}

