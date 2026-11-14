using ET;
using GameLogic;
using UnityEngine;
using UnityEngine.UI;
using XEngine.UI;


namespace ET.Client
{
    [EntitySystemOf(typeof(UISkillComponent))]
    [FriendOf(typeof(UISkillComponent))]
    public static partial class UISkillComponentSystem
    {
        [EntitySystem]
        private static void Awake(this UISkillComponent self)
        {
            UIBindComponent m_bindComponent = self.GetParent<UI>().GameObject.GetComponent<UIBindComponent>();
            self.m_btnClose = m_bindComponent.GetComponent<Button>(0);
            self.m_btnClose.onClick.AddListener(() => { self.OnClose(); });
        }

        [EntitySystem]
        private static void Destroy(this UISkillComponent self)
        {
        }
        
        
        public static void OnClose(this UISkillComponent self)
        {
            self.Scene().GetComponent<UIComponent>().Remove(UIType.UISkill);
        }
    }
}