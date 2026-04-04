using System.Collections.Generic;
using UnityEngine;

namespace ET.Client
{
    [ComponentOf(typeof(Scene))]
    public class UIGlobalComponent: Entity, IAwake,IDestroy
    {
        public string UIRootPath = $"Assets/Bundles/Tools/UIRoot.prefab";
        public Transform UIRootObj;

        public Transform UICanvas;
        //public Dictionary<int, Transform> UILayers = new Dictionary<int, Transform>();
    }
}