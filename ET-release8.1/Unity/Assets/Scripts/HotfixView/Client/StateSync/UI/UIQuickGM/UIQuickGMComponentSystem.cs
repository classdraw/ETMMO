using ET;
using GameLogic;
using TEngine;
using UnityEngine;
using UnityEngine.UI;
using XEngine.UI;

namespace ET.Client
{
    [EntitySystemOf(typeof(UIQuickGMComponent))]
    [FriendOf(typeof(UIQuickGMComponent))]
    public static partial class UIQuickGMComponentSystem
    {
        private const float PanelPosCollapsed = -200f;
        private const float PanelPosExpanded = 200f;
        private const float PanelTweenDuration = 0.25f;

        [EntitySystem]
        private static void Awake(this UIQuickGMComponent self)
        {
            UIBindComponent m_bindComponent = self.GetParent<UI>().GameObject.GetComponent<UIBindComponent>();
            self.m_goPanel = m_bindComponent.GetGameObject(0);
            self.m_panelRect = self.m_goPanel.GetComponent<RectTransform>();
            self.m_btnTap1 = m_bindComponent.GetComponent<Button>(1);
            self.m_btnTap1.onClick.AddListener(() => { self.OnTap1(); });
            self.m_btnTap2 = m_bindComponent.GetComponent<Button>(2);
            self.m_btnTap2.onClick.AddListener(() => { self.OnTap2(); });
            self.m_goP1 = m_bindComponent.GetGameObject(3);
            self.m_btnP1SetHPSPFull = m_bindComponent.GetComponent<Button>(4);
            self.m_btnP1SetHPSPFull.onClick.AddListener(() => { self.OnP1SetHPSPFull(); });
            self.m_goP2 = m_bindComponent.GetGameObject(5);
            self.m_btnClickOpen = m_bindComponent.GetComponent<Button>(6);
            self.m_btnClickOpen.onClick.AddListener(() => { self.OnClickOpen(); });
            self.m_btnClickClose = m_bindComponent.GetComponent<Button>(7);
            self.m_btnClickClose.onClick.AddListener(() => { self.OnClickClose(); });
            
            self.m_isExpanded = false;
            self.ApplyCollapsedVisual(immediate: true);
            self.OnTap1();
        }

        [EntitySystem]
        private static void Destroy(this UIQuickGMComponent self)
        {
            if (self.m_panelRect != null)
            {
                Utility.Tween.StopAll(self.m_panelRect);
            }
        }
        
        public static void OnTap1(this UIQuickGMComponent self)
        {
            self.HideAllContentPanels();
            self.m_goP1.SetActive(true);
        }

        public static void OnTap2(this UIQuickGMComponent self)
        {
            self.HideAllContentPanels();
            self.m_goP2.SetActive(true);
        }

        private static void HideAllContentPanels(this UIQuickGMComponent self)
        {
            self.m_goP1.SetActive(false);
            self.m_goP2.SetActive(false);
        }
        
        public static void OnP1SetHPSPFull(this UIQuickGMComponent self)
        {
            BattleHelper.GMSetHPSPFull(self.Root()).Coroutine();
        }

        public static void OnClickOpen(this UIQuickGMComponent self)
        {
            if (self.m_isExpanded)
            {
                return;
            }

            self.m_isExpanded = true;
            self.m_btnClickOpen.gameObject.SetActive(false);
            self.m_btnClickClose.gameObject.SetActive(true);
            self.TweenPanelX(PanelPosExpanded);
        }

        public static void OnClickClose(this UIQuickGMComponent self)
        {
            if (!self.m_isExpanded)
            {
                return;
            }

            self.m_isExpanded = false;
            self.m_btnClickOpen.gameObject.SetActive(true);
            self.m_btnClickClose.gameObject.SetActive(false);
            self.TweenPanelX(PanelPosCollapsed);
        }

        private static void ApplyCollapsedVisual(this UIQuickGMComponent self, bool immediate)
        {
            self.m_btnClickOpen.gameObject.SetActive(true);
            self.m_btnClickClose.gameObject.SetActive(false);
            self.SetPanelPosX(PanelPosCollapsed, immediate);
        }

        private static void TweenPanelX(this UIQuickGMComponent self, float targetX)
        {
            self.SetPanelPosX(targetX, immediate: false);
        }

        private static void SetPanelPosX(this UIQuickGMComponent self, float targetX, bool immediate)
        {
            RectTransform rect = self.m_panelRect;
            if (rect == null)
            {
                return;
            }

            Utility.Tween.StopAll(rect);
            if (immediate)
            {
                Vector2 pos = rect.anchoredPosition;
                pos.x = targetX;
                rect.anchoredPosition = pos;
                return;
            }

            Utility.Tween.UIAnchoredPositionX(rect, targetX, PanelTweenDuration);
        }
    }
}