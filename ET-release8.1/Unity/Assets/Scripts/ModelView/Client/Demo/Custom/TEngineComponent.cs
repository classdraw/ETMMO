using System;
using UnityEngine;
using TEngine;

namespace ET.Client
{
    [ComponentOf(typeof(Scene))]
    public class TEngineComponent: Entity, IAwake,IDestroy
    {
        public string GameEntryPath = $"Assets/Bundles/Tools/GameEntry.prefab";
        public string UIRootPath = $"Assets/Bundles/Tools/UIRoot.prefab";
        public GameObject GameEntryObj;
        public GameObject UIRootObj;
        public TEngineGlobal EngineGlobal;
        
        //UIModule
    }
}