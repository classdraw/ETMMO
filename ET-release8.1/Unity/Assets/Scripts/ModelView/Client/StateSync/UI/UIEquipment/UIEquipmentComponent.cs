using ET;
using GameLogic;
using UnityEngine;
using UnityEngine.UI;

namespace ET.Client
{
    [ComponentOf(typeof(UI))]
    public class UIEquipmentComponent: Entity, IAwake, IDestroy
    {
        public GameObject m_goStateWindow;
        public Button m_btnRightOption;
        public Text m_textCName;
        public Image m_imgCImage;
        public Button m_btnClose;
        public Button m_btnLeftOption;
    }
}