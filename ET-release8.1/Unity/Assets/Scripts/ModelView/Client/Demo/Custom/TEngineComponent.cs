using System;
using UnityEngine;
using TEngine;

namespace ET.Client
{
    [ComponentOf(typeof(Scene))]
    public class TEngineComponent: Entity, IAwake,IDestroy
    {
        public string GameEntryPath = $"Assets/Bundles/Tools/GameEntry.prefab";

        public GameObject GameEntryObj;
    }
}