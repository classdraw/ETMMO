using ET;
using GameLogic;
using UnityEngine;
using UnityEngine.UI;

namespace ET.Client
{
    [EntitySystemOf(typeof(UILoadingComponent))]
    [FriendOf(typeof(UILoadingComponent))]
    public static partial class UILoadingComponentSystem
    {
        [EntitySystem]
        private static void Awake(this UILoadingComponent self)
        {
            UIBindComponent m_bindComponent = self.GetParent<UI>().GameObject.GetComponent<UIBindComponent>();
            self.m_textLoading = m_bindComponent.GetComponent<Text>(0);
        }
        
        [EntitySystem]
        private static void Destroy(this UILoadingComponent self)
        {

        }

    }
}

