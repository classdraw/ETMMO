using System.Collections.Generic;
using UnityEngine;

namespace ET.Client
{
    public struct MountItem
    {
        public GameObject Object;
        public string PoolKey;
        public long WaitDestroyTime;
    }

    /// <summary>
    /// 挂载系统，和GameObjectComponent共存，挂特效等东西
    /// </summary>
    [ComponentOf(typeof(Unit))]
    public class MountComponent:Entity,IAwake,IUpdate,IDestroy
    {
        public List<MountItem> Items = new();
    }
}
