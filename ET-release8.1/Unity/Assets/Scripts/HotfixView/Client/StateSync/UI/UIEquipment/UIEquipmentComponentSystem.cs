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
            self.m_textStrValue = m_bindComponent.GetComponent<Text>(1);
            self.m_textAgiValue = m_bindComponent.GetComponent<Text>(2);
            self.m_textVitValue = m_bindComponent.GetComponent<Text>(3);
            self.m_textIntValue = m_bindComponent.GetComponent<Text>(4);
            self.m_textDexValue = m_bindComponent.GetComponent<Text>(5);
            self.m_textLukValue = m_bindComponent.GetComponent<Text>(6);
            self.m_textPhysicAttack = m_bindComponent.GetComponent<Text>(7);
            self.m_textPhysicDef = m_bindComponent.GetComponent<Text>(8);
            self.m_textMagicAttack = m_bindComponent.GetComponent<Text>(9);
            self.m_textMagicDef = m_bindComponent.GetComponent<Text>(10);
            self.m_textHit = m_bindComponent.GetComponent<Text>(11);
            self.m_textMiss = m_bindComponent.GetComponent<Text>(12);
            self.m_btnRightOption = m_bindComponent.GetComponent<Button>(13);
            self.m_btnRightOption.onClick.AddListener(() => { self.OnRightOption(); });
            self.m_textCName = m_bindComponent.GetComponent<Text>(14);
            self.m_imgCImage = m_bindComponent.GetComponent<Image>(15);
            self.m_btnClose = m_bindComponent.GetComponent<Button>(16);
            self.m_btnClose.onClick.AddListener(() => { self.OnClose(); });
            self.m_btnLeftOption = m_bindComponent.GetComponent<Button>(17);
            self.m_btnLeftOption.onClick.AddListener(() => { self.OnLeftOption(); });
            
            self.SetStateWindowExpanded(true);
            self.RefreshPlayerInfo();
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
            self.Scene().GetComponent<UIComponent>().Remove(UIType.UIEquipment);
        }

        /// <summary>刷新角色名与属性面板数值。</summary>
        public static void RefreshPlayerInfo(this UIEquipmentComponent self)
        {
            if (self == null || self.IsDisposed)
            {
                return;
            }

            Scene currentScene = self.Root()?.CurrentScene();
            if (currentScene == null)
            {
                return;
            }

            Unit myUnit = UnitHelper.GetMyUnitFromCurrentScene(currentScene);
            if (myUnit == null || myUnit.IsDisposed)
            {
                return;
            }

            if (self.m_textCName != null)
            {
                self.m_textCName.text = myUnit.Name ?? string.Empty;
            }

            NumericComponent numericComponent = myUnit.GetComponent<NumericComponent>();
            if (numericComponent == null)
            {
                return;
            }

            SetStatText(self.m_textStrValue, numericComponent.GetAsInt(NumericType.STR));
            SetStatText(self.m_textAgiValue, numericComponent.GetAsInt(NumericType.AGI));
            SetStatText(self.m_textVitValue, numericComponent.GetAsInt(NumericType.VIT));
            SetStatText(self.m_textIntValue, numericComponent.GetAsInt(NumericType.INT));
            SetStatText(self.m_textDexValue, numericComponent.GetAsInt(NumericType.DEX));
            SetStatText(self.m_textLukValue, numericComponent.GetAsInt(NumericType.LUK));
            SetRangeStatText(self.m_textPhysicAttack, numericComponent, NumericType.Atk, NumericType.AtkRandom);
            SetRangeStatText(self.m_textPhysicDef, numericComponent, NumericType.Def, NumericType.DefRandom);
            SetRangeStatText(self.m_textMagicAttack, numericComponent, NumericType.MAtk, NumericType.MAtkRandom);
            SetRangeStatText(self.m_textMagicDef, numericComponent, NumericType.MDef, NumericType.MDefRandom);
            SetStatText(self.m_textHit, numericComponent.GetAsInt(NumericType.Hit));
            SetStatText(self.m_textMiss, numericComponent.GetAsInt(NumericType.Flee));
        }

        private static void SetStatText(Text text, int value)
        {
            if (text == null)
            {
                return;
            }

            text.text = value.ToString();
        }

        private static void SetRangeStatText(Text text, NumericComponent numeric, int valueType, int randomType)
        {
            if (text == null)
            {
                return;
            }

            int value = numeric.GetAsInt(valueType);
            int random = numeric.GetAsInt(randomType);
            text.text = random > 0 ? $"{value}-{value + random}" : value.ToString();
        }
    }
}