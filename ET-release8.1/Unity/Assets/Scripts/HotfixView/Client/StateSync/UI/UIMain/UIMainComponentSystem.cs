using ET;
using GameLogic;
using UnityEngine;
using UnityEngine.UI;


namespace ET.Client
{
    [EntitySystemOf(typeof(UIMainComponent))]
    [FriendOf(typeof(UIMainComponent))]
    public static partial class UIMainComponentSystem
    {
        [EntitySystem]
        private static void Awake(this UIMainComponent self)
        {
            UIBindComponent m_bindComponent = self.GetParent<UI>().GameObject.GetComponent<UIBindComponent>();
            self.m_btnBag = m_bindComponent.GetComponent<Button>(0);
            self.m_btnBag.onClick.AddListener(() => { self.OnBag(); });
        }
        
        [EntitySystem]
        private static void Destroy(this UIMainComponent self)
        {
        }

        public static void OnBag(this UIMainComponent self)
        {
            UIHelper.Create(self.Scene(), UIType.UIBag).Coroutine();
        }
    }
}