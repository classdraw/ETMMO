using ET;
using GameLogic;
using UnityEngine;
using UnityEngine.UI;

namespace ET.Client
{
    [ComponentOf(typeof(UI))]
    public class UIQuickGMComponent: Entity, IAwake, IDestroy
    {
        public GameObject m_goPanel;
        public RectTransform m_panelRect;
        public bool m_isExpanded;
        public Button m_btnTap1;
        public Button m_btnTap2;
        public GameObject m_goP1;
        public Button m_btnP1SetHPSPFull;
        public GameObject m_goP2;
        public Button m_btnClickOpen;
        public Button m_btnClickClose;
    }
}