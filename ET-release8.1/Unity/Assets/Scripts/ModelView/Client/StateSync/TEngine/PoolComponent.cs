using System.Collections.Generic;
using UnityEngine;

namespace ET.Client
{
    [ComponentOf(typeof(Scene))]
    public class PoolComponent:Entity,IAwake,IDestroy
    {
        public Transform PoolRoot;
        public Dictionary<string, List<GameObject>> Pools = new Dictionary<string, List<GameObject>>();
    }
}

