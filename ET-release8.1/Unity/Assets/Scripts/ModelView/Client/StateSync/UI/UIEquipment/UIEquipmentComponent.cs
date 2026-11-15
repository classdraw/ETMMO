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
        public Text m_textStrValue;
        public Text m_textAgiValue;
        public Text m_textVitValue;
        public Text m_textIntValue;
        public Text m_textDexValue;
        public Text m_textLukValue;
        public Text m_textPhysicAttack;
        public Text m_textPhysicDef;
        public Text m_textMagicAttack;
        public Text m_textMagicDef;
        public Text m_textHit;
        public Text m_textMiss;
        public Button m_btnRightOption;
        public Text m_textCName;
        public Image m_imgCImage;
        public Button m_btnClose;
        public Button m_btnLeftOption;
    }
}