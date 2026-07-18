using System.Collections.Generic;
using UnityEngine;

namespace ET.Client
{
    /// <summary>
    /// 挂载系统，和GameObjectComponent共存，挂特效等东西
    /// </summary>
    [ComponentOf(typeof(Unit))]
    public class MountComponent:Entity,IAwake,IUpdate,IDestroy
    {
        public List<GameObject> Objects = new();
        public List<long> WaitDestroyTimes = new();
    }
}

