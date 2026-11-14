using ET;
using GameLogic;
using UnityEngine;
using UnityEngine.UI;

namespace ET.Client
{
    [ComponentOf(typeof(UI))]
    public class UISkillComponent: Entity, IAwake, IDestroy
    {
        public Button m_btnClose;
    }
    
}