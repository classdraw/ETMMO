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
			self.m_textNeedStrValue = m_bindComponent.GetComponent<Text>(2);
			self.m_btnStrAdd = m_bindComponent.GetComponent<Button>(3);
			self.m_btnStrAdd.onClick.AddListener(() => { self.OnStrAdd(); });
			self.m_textAgiValue = m_bindComponent.GetComponent<Text>(4);
			self.m_textNeedAgiValue = m_bindComponent.GetComponent<Text>(5);
			self.m_btnAgiAdd = m_bindComponent.GetComponent<Button>(6);
			self.m_btnAgiAdd.onClick.AddListener(() => { self.OnAgiAdd(); });
			self.m_textVitValue = m_bindComponent.GetComponent<Text>(7);
			self.m_textNeedVitValue = m_bindComponent.GetComponent<Text>(8);
			self.m_btnVitAdd = m_bindComponent.GetComponent<Button>(9);
			self.m_btnVitAdd.onClick.AddListener(() => { self.OnVitAdd(); });
			self.m_textIntValue = m_bindComponent.GetComponent<Text>(10);
			self.m_textNeedIntValue = m_bindComponent.GetComponent<Text>(11);
			self.m_btnIntAdd = m_bindComponent.GetComponent<Button>(12);
			self.m_btnIntAdd.onClick.AddListener(() => { self.OnIntAdd(); });
			self.m_textDexValue = m_bindComponent.GetComponent<Text>(13);
			self.m_textNeedDexValue = m_bindComponent.GetComponent<Text>(14);
			self.m_btnDexAdd = m_bindComponent.GetComponent<Button>(15);
			self.m_btnDexAdd.onClick.AddListener(() => { self.OnDexAdd(); });
			self.m_textLukValue = m_bindComponent.GetComponent<Text>(16);
			self.m_textNeedLukValue = m_bindComponent.GetComponent<Text>(17);
			self.m_btnLukAdd = m_bindComponent.GetComponent<Button>(18);
			self.m_btnLukAdd.onClick.AddListener(() => { self.OnLukAdd(); });
			self.m_textPoint = m_bindComponent.GetComponent<Text>(19);
			self.m_textPhysicAttack = m_bindComponent.GetComponent<Text>(20);
			self.m_textPhysicDef = m_bindComponent.GetComponent<Text>(21);
			self.m_textMagicAttack = m_bindComponent.GetComponent<Text>(22);
			self.m_textMagicDef = m_bindComponent.GetComponent<Text>(23);
			self.m_textHit = m_bindComponent.GetComponent<Text>(24);
			self.m_textMiss = m_bindComponent.GetComponent<Text>(25);
			self.m_btnRightOption = m_bindComponent.GetComponent<Button>(26);
			self.m_btnRightOption.onClick.AddListener(() => { self.OnRightOption(); });
			self.m_textCName = m_bindComponent.GetComponent<Text>(27);
			self.m_textCLevel = m_bindComponent.GetComponent<Text>(28);
			self.m_textCJob = m_bindComponent.GetComponent<Text>(29);
			self.m_imgCImage = m_bindComponent.GetComponent<Image>(30);
			self.m_btnClose = m_bindComponent.GetComponent<Button>(31);
			self.m_btnClose.onClick.AddListener(() => { self.OnClose(); });
			self.m_btnLeftOption = m_bindComponent.GetComponent<Button>(32);
			self.m_btnLeftOption.onClick.AddListener(() => { self.OnLeftOption(); });
			
            self.SetStateWindowExpanded(true);
            self.RefreshPlayerInfo();
            self.DeferredRefreshPlayerInfoAfterLocalization().Coroutine();
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

        private static async ETTask DeferredRefreshPlayerInfoAfterLocalization(this UIEquipmentComponent self)
        {
            Scene root = self.Root();
            TimerComponent timer = root?.GetComponent<TimerComponent>();
            if (timer == null)
            {
                return;
            }

            const string levelTerm = UILocalizationHelper.EquipmentLevelTerm;
            for (int i = 0; i < 120; i++)
            {
                if (self == null || self.IsDisposed)
                {
                    return;
                }

                if (UILocalizationHelper.IsTermResolvable(levelTerm))
                {
                    self.RefreshPlayerInfo();
                    return;
                }

                await timer.WaitFrameAsync();
            }

            self.RefreshPlayerInfo();
        }

        /// <summary>刷新角色名、等级、职业与属性面板数值。</summary>
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

            if (self.m_textCJob != null)
            {
                self.m_textCJob.text = ResolveJobDisplayName(myUnit);
            }

            NumericComponent numericComponent = myUnit.GetComponent<NumericComponent>();
            if (numericComponent == null)
            {
                if (self.m_textCLevel != null)
                {
                    self.m_textCLevel.text = string.Empty;
                }

                return;
            }

            if (self.m_textCLevel != null)
            {
                int level = numericComponent.GetAsInt(NumericType.Level);
                self.m_textCLevel.text = UILocalizationHelper.GetTerm(UILocalizationHelper.EquipmentLevelTerm, "Level:{0}", level);
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

        private static string ResolveJobDisplayName(Unit unit)
        {
            if (unit == null)
            {
                return string.Empty;
            }

            int job = 0;
            if (RoleCreateProfileHelper.TryParseProfileString(unit.BaseExternalDisplay, out ExternalDisplayAppearance profile))
            {
                job = profile.Job;
            }

            if (job <= 0)
            {
                job = JobConfigHelper.GetDefaultJobId();
            }
            else
            {
                job = JobConfigHelper.NormalizeJobId(job);
            }

            return LoginRoleDisplayHelper.GetJobName(job);
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
        
        
        public static void OnStrAdd(this UIEquipmentComponent self)
        {
        }

        public static void OnAgiAdd(this UIEquipmentComponent self)
        {
        }

        public static void OnVitAdd(this UIEquipmentComponent self)
        {
        }

        public static void OnIntAdd(this UIEquipmentComponent self)
        {
        }

        public static void OnDexAdd(this UIEquipmentComponent self)
        {
        }

        public static void OnLukAdd(this UIEquipmentComponent self)
        {
        }

    }
}