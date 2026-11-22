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
        public Text m_textNeedStrValue;
        public Button m_btnStrRight;
        public Button m_btnStrLeft;
        public Text m_textAgiValue;
        public Text m_textNeedAgiValue;
        public Button m_btnAgiRight;
        public Button m_btnAgiLeft;
        public Text m_textVitValue;
        public Text m_textNeedVitValue;
        public Button m_btnVitRight;
        public Button m_btnVitLeft;
        public Text m_textIntValue;
        public Text m_textNeedIntValue;
        public Button m_btnIntRight;
        public Button m_btnIntLeft;
        public Text m_textDexValue;
        public Text m_textNeedDexValue;
        public Button m_btnDexRight;
        public Button m_btnDexLeft;
        public Text m_textLukValue;
        public Text m_textNeedLukValue;
        public Button m_btnLukRight;
        public Button m_btnLukLeft;
        public Text m_textPoint;
        public Text m_textPhysicAttack;
        public Text m_textPhysicDef;
        public Text m_textMagicAttack;
        public Text m_textMagicDef;
        public Text m_textHit;
        public Text m_textMiss;
        public Button m_btnRightOption;
        public Text m_textCName;
        public Text m_textCLevel;
        public Text m_textCJob;
        public Image m_imgCImage;
        public Button m_btnClose;
        public Button m_btnLeftOption;

        /// <summary>关闭界面前预分配，不立刻写数值。</summary>
        public int PendingStr;
        public int PendingAgi;
        public int PendingVit;
        public int PendingInt;
        public int PendingDex;
        public int PendingLuk;
        public bool CommittingOnClose;
    }
}