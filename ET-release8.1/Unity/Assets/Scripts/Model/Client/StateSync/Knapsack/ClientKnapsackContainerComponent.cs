using System.Collections.Generic;

namespace ET.Client
{
    [ChildOf(typeof(ClientKnapsackComponent))]
    public class ClientKnapsackContainerComponent :Entity,IAwake<int>,IDestroy
    {
        public int KnapsackContainerType { get; set; }//背包还是装备 还是仓库
        public Dictionary<long, EntityRef<Item>> Items = new Dictionary<long, EntityRef<Item>>();
    }
}

