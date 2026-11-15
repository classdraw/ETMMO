using ET;
using GameLogic;
using UnityEngine;
using UnityEngine.UI;
using XEngine.UI;

namespace ET.Client
{
    [EntitySystemOf(typeof(UIEquipmentComponent))]
    [FriendOf(typeof(UIEquipmentComponent))]
    public static partial class UIEquipmentComponentSystem
    {
        [EntitySystem]
        private static void Awake(this UIEquipmentComponent self)
        {
            UIBindComponent m_bindComponent = self.GetParent<UI>().GameObject.GetComponent<UIBindComponent>();
            self.m_goStateWindow = m_bindComponent.GetGameObject(0);
            self.m_btnRightOption = m_bindComponent.GetComponent<Button>(1);
            self.m_btnRightOption.onClick.AddListener(() => { self.OnRightOption(); });
            self.m_textCName = m_bindComponent.GetComponent<Text>(2);
            self.m_imgCImage = m_bindComponent.GetComponent<Image>(3);
            self.m_btnClose = m_bindComponent.GetComponent<Button>(4);
            self.m_btnClose.onClick.AddListener(() => { self.OnClose(); });
            self.m_btnLeftOption = m_bindComponent.GetComponent<Button>(5);
            self.m_btnLeftOption.onClick.AddListener(() => { self.OnLeftOption(); });

            self.SetStateWindowExpanded(true);
        }

        [EntitySystem]
        private static void Destroy(this UIEquipmentComponent self)
        {
        }

        /// <summary>展开：右侧按钮 + 属性面板；收起：左侧按钮，隐藏面板。</summary>
        private static void SetStateWindowExpanded(this UIEquipmentComponent self, bool expanded)
        {
            if (self.m_goStateWindow != null)
            {
                self.m_goStateWindow.SetActive(expanded);
            }

            if (self.m_btnRightOption != null)
            {
                self.m_btnRightOption.gameObject.SetActive(expanded);
            }

            if (self.m_btnLeftOption != null)
            {
                self.m_btnLeftOption.gameObject.SetActive(!expanded);
            }
        }

        public static void OnLeftOption(this UIEquipmentComponent self)
        {
            self.SetStateWindowExpanded(true);
        }

        public static void OnRightOption(this UIEquipmentComponent self)
        {
            self.SetStateWindowExpanded(false);
        }

        public static void OnClose(this UIEquipmentComponent self)
        {
            self.SetStateWindowExpanded(false);
        }
    }
}