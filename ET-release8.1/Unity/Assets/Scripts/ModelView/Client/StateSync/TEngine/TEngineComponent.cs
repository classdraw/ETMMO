using System;
using System.Collections.Generic;
using UnityEngine;
using TEngine;

namespace ET.Client
{
    [ComponentOf(typeof(Scene))]
    public class TEngineComponent: Entity, IAwake,IDestroy
    {
        public string GameEntryPath = $"Assets/Bundles/Tools/GameEntry.prefab";
        
        public GameObject GameEntryObj;
        
        public TEngineGlobal EngineGlobal;
        
        //UIModule


        #region Setting的key

        public Dictionary<Setting_Key_Enum, float> SettingValues = new Dictionary<Setting_Key_Enum, float>();
        #endregion
    }
}