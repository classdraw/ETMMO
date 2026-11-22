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
        private const string PendingStatColor = "#3CB371";

        [EntitySystem]
        private static void Awake(this UIEquipmentComponent self)
        {
			UIBindComponent m_bindComponent = self.GetParent<UI>().GameObject.GetComponent<UIBindComponent>();
			self.m_goStateWindow = m_bindComponent.GetGameObject(0);
			self.m_textStrValue = m_bindComponent.GetComponent<Text>(1);
			self.m_textNeedStrValue = m_bindComponent.GetComponent<Text>(2);
			self.m_btnStrRight = m_bindComponent.GetComponent<Button>(3);
			self.m_btnStrRight.onClick.AddListener(() => { self.OnStrRight(); });
			self.m_btnStrLeft = m_bindComponent.GetComponent<Button>(4);
			self.m_btnStrLeft.onClick.AddListener(() => { self.OnStrLeft(); });
			self.m_textAgiValue = m_bindComponent.GetComponent<Text>(5);
			self.m_textNeedAgiValue = m_bindComponent.GetComponent<Text>(6);
			self.m_btnAgiRight = m_bindComponent.GetComponent<Button>(7);
			self.m_btnAgiRight.onClick.AddListener(() => { self.OnAgiRight(); });
			self.m_btnAgiLeft = m_bindComponent.GetComponent<Button>(8);
			self.m_btnAgiLeft.onClick.AddListener(() => { self.OnAgiLeft(); });
			self.m_textVitValue = m_bindComponent.GetComponent<Text>(9);
			self.m_textNeedVitValue = m_bindComponent.GetComponent<Text>(10);
			self.m_btnVitRight = m_bindComponent.GetComponent<Button>(11);
			self.m_btnVitRight.onClick.AddListener(() => { self.OnVitRight(); });
			self.m_btnVitLeft = m_bindComponent.GetComponent<Button>(12);
			self.m_btnVitLeft.onClick.AddListener(() => { self.OnVitLeft(); });
			self.m_textIntValue = m_bindComponent.GetComponent<Text>(13);
			self.m_textNeedIntValue = m_bindComponent.GetComponent<Text>(14);
			self.m_btnIntRight = m_bindComponent.GetComponent<Button>(15);
			self.m_btnIntRight.onClick.AddListener(() => { self.OnIntRight(); });
			self.m_btnIntLeft = m_bindComponent.GetComponent<Button>(16);
			self.m_btnIntLeft.onClick.AddListener(() => { self.OnIntLeft(); });
			self.m_textDexValue = m_bindComponent.GetComponent<Text>(17);
			self.m_textNeedDexValue = m_bindComponent.GetComponent<Text>(18);
			self.m_btnDexRight = m_bindComponent.GetComponent<Button>(19);
			self.m_btnDexRight.onClick.AddListener(() => { self.OnDexRight(); });
			self.m_btnDexLeft = m_bindComponent.GetComponent<Button>(20);
			self.m_btnDexLeft.onClick.AddListener(() => { self.OnDexLeft(); });
			self.m_textLukValue = m_bindComponent.GetComponent<Text>(21);
			self.m_textNeedLukValue = m_bindComponent.GetComponent<Text>(22);
            self.m_btnLukRight = m_bindComponent.GetComponent<Button>(23);
            self.m_btnLukRight.onClick.AddListener(() => { self.OnLukRight(); });
			self.m_btnLukLeft = m_bindComponent.GetComponent<Button>(24);
			self.m_btnLukLeft.onClick.AddListener(() => { self.OnLukLeft(); });
			self.m_textPoint = m_bindComponent.GetComponent<Text>(25);
			self.m_textPhysicAttack = m_bindComponent.GetComponent<Text>(26);
			self.m_textPhysicDef = m_bindComponent.GetComponent<Text>(27);
			self.m_textMagicAttack = m_bindComponent.GetComponent<Text>(28);
			self.m_textMagicDef = m_bindComponent.GetComponent<Text>(29);
			self.m_textHit = m_bindComponent.GetComponent<Text>(30);
			self.m_textMiss = m_bindComponent.GetComponent<Text>(31);
			self.m_btnRightOption = m_bindComponent.GetComponent<Button>(32);
			self.m_btnRightOption.onClick.AddListener(() => { self.OnRightOption(); });
			self.m_textCName = m_bindComponent.GetComponent<Text>(33);
			self.m_textCLevel = m_bindComponent.GetComponent<Text>(34);
			self.m_textCJob = m_bindComponent.GetComponent<Text>(35);
			self.m_imgCImage = m_bindComponent.GetComponent<Image>(36);
			self.m_btnClose = m_bindComponent.GetComponent<Button>(37);
			self.m_btnClose.onClick.AddListener(() => { self.OnClose(); });
			self.m_btnLeftOption = m_bindComponent.GetComponent<Button>(38);
			self.m_btnLeftOption.onClick.AddListener(() => { self.OnLeftOption(); });

            self.EnsureStatAdjustButtonsVisible();
			
            self.ClearPendingAllocation();
            self.SetStateWindowExpanded(true);
            self.RefreshPlayerInfo();
            self.DeferredRefreshPlayerInfoAfterLocalization().Coroutine();
        }

        [EntitySystem]
        private static void Destroy(this UIEquipmentComponent self)
        {
            self.ClearPendingAllocation();
        }

        private static void ClearPendingAllocation(this UIEquipmentComponent self)
        {
            self.PendingStr = 0;
            self.PendingAgi = 0;
            self.PendingVit = 0;
            self.PendingInt = 0;
            self.PendingDex = 0;
            self.PendingLuk = 0;
            self.CommittingOnClose = false;
        }

        private static bool HasPendingAllocation(this UIEquipmentComponent self)
        {
            return self.PendingStr > 0 || self.PendingAgi > 0 || self.PendingVit > 0 || self.PendingInt > 0 ||
                   self.PendingDex > 0 || self.PendingLuk > 0;
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

        /// <summary>关闭装备界面：有预分配时先向服务端提交，成功后再关界面。</summary>
        public static void OnClose(this UIEquipmentComponent self)
        {
            self.RequestCloseAsync().Coroutine();
        }

        public static async ETTask RequestCloseAsync(this UIEquipmentComponent self)
        {
            if (self == null || self.IsDisposed)
            {
                return;
            }

            if (!self.HasPendingAllocation())
            {
                self.RemoveEquipmentUi();
                return;
            }

            if (self.CommittingOnClose)
            {
                return;
            }

            await self.CommitPendingAndClose();
        }

        private static void RemoveEquipmentUi(this UIEquipmentComponent self)
        {
            self.Scene().GetComponent<UIComponent>().Remove(UIType.UIEquipment);
        }

        private static async ETTask CommitPendingAndClose(this UIEquipmentComponent self)
        {
            self.CommittingOnClose = true;
            try
            {
                bool ok = await AttributePointsHelper.CommitAttributePoints(
                    self.Root(),
                    self.PendingStr,
                    self.PendingAgi,
                    self.PendingVit,
                    self.PendingInt,
                    self.PendingDex,
                    self.PendingLuk);
                if (!ok || self == null || self.IsDisposed)
                {
                    return;
                }

                self.ClearPendingAllocation();
                self.RemoveEquipmentUi();
            }
            finally
            {
                if (self != null && !self.IsDisposed)
                {
                    self.CommittingOnClose = false;
                }
            }
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

            int str = numericComponent.GetAsInt(NumericType.STR);
            int agi = numericComponent.GetAsInt(NumericType.AGI);
            int vit = numericComponent.GetAsInt(NumericType.VIT);
            int intell = numericComponent.GetAsInt(NumericType.INT);
            int dex = numericComponent.GetAsInt(NumericType.DEX);
            int luk = numericComponent.GetAsInt(NumericType.LUK);
            int serverRemaining = numericComponent.GetAsInt(NumericType.RemainingAttributePoints);

            int pendingSpent = ComputePendingPointsSpent(self, str, agi, vit, intell, dex, luk);
            int allocatableRemaining = serverRemaining - pendingSpent;

            SetPrimaryAttributeStatText(self.m_textStrValue, numericComponent, NumericType.STR, self.PendingStr);
            SetPrimaryAttributeStatText(self.m_textAgiValue, numericComponent, NumericType.AGI, self.PendingAgi);
            SetPrimaryAttributeStatText(self.m_textVitValue, numericComponent, NumericType.VIT, self.PendingVit);
            SetPrimaryAttributeStatText(self.m_textIntValue, numericComponent, NumericType.INT, self.PendingInt);
            SetPrimaryAttributeStatText(self.m_textDexValue, numericComponent, NumericType.DEX, self.PendingDex);
            SetPrimaryAttributeStatText(self.m_textLukValue, numericComponent, NumericType.LUK, self.PendingLuk);

            SetStatText(self.m_textNeedStrValue, RoClassicLevelHelper.GetAttributePointCostToIncrease(str + self.PendingStr));
            SetStatText(self.m_textNeedAgiValue, RoClassicLevelHelper.GetAttributePointCostToIncrease(agi + self.PendingAgi));
            SetStatText(self.m_textNeedVitValue, RoClassicLevelHelper.GetAttributePointCostToIncrease(vit + self.PendingVit));
            SetStatText(self.m_textNeedIntValue, RoClassicLevelHelper.GetAttributePointCostToIncrease(intell + self.PendingInt));
            SetStatText(self.m_textNeedDexValue, RoClassicLevelHelper.GetAttributePointCostToIncrease(dex + self.PendingDex));
            SetStatText(self.m_textNeedLukValue, RoClassicLevelHelper.GetAttributePointCostToIncrease(luk + self.PendingLuk));

            // 预制体左侧 TextPoint（I2）负责「剩余点数：」，此处仅写数字
            if (self.m_textPoint != null)
            {
                self.m_textPoint.text = Mathf.Max(0, allocatableRemaining).ToString();
            }

            SetStatAdjustButtons(self.m_btnStrLeft, self.m_btnStrRight, self.PendingStr,
                RoClassicLevelHelper.GetAttributePointCostToIncrease(str + self.PendingStr), allocatableRemaining);
            SetStatAdjustButtons(self.m_btnAgiLeft, self.m_btnAgiRight, self.PendingAgi,
                RoClassicLevelHelper.GetAttributePointCostToIncrease(agi + self.PendingAgi), allocatableRemaining);
            SetStatAdjustButtons(self.m_btnVitLeft, self.m_btnVitRight, self.PendingVit,
                RoClassicLevelHelper.GetAttributePointCostToIncrease(vit + self.PendingVit), allocatableRemaining);
            SetStatAdjustButtons(self.m_btnIntLeft, self.m_btnIntRight, self.PendingInt,
                RoClassicLevelHelper.GetAttributePointCostToIncrease(intell + self.PendingInt), allocatableRemaining);
            SetStatAdjustButtons(self.m_btnDexLeft, self.m_btnDexRight, self.PendingDex,
                RoClassicLevelHelper.GetAttributePointCostToIncrease(dex + self.PendingDex), allocatableRemaining);
            SetStatAdjustButtons(self.m_btnLukLeft, self.m_btnLukRight, self.PendingLuk,
                RoClassicLevelHelper.GetAttributePointCostToIncrease(luk + self.PendingLuk), allocatableRemaining);

            RefreshDerivedCombatDisplay(self, myUnit, numericComponent);
        }

        private static int ComputePendingPointsSpent(
            UIEquipmentComponent self, int str, int agi, int vit, int intell, int dex, int luk)
        {
            int total = 0;
            total += RoClassicLevelHelper.ComputeAttributePointCostForIncreaseRange(str, self.PendingStr);
            total += RoClassicLevelHelper.ComputeAttributePointCostForIncreaseRange(agi, self.PendingAgi);
            total += RoClassicLevelHelper.ComputeAttributePointCostForIncreaseRange(vit, self.PendingVit);
            total += RoClassicLevelHelper.ComputeAttributePointCostForIncreaseRange(intell, self.PendingInt);
            total += RoClassicLevelHelper.ComputeAttributePointCostForIncreaseRange(dex, self.PendingDex);
            total += RoClassicLevelHelper.ComputeAttributePointCostForIncreaseRange(luk, self.PendingLuk);
            return total;
        }

        private static bool TryIncreasePending(this UIEquipmentComponent self, ref int pending, int baseValue, int allocatableRemaining)
        {
            int nextCost = RoClassicLevelHelper.GetAttributePointCostToIncrease(baseValue + pending);
            if (allocatableRemaining < nextCost)
            {
                return false;
            }

            pending++;
            self.RefreshPlayerInfo();
            return true;
        }

        private static bool TryDecreasePending(this UIEquipmentComponent self, ref int pending)
        {
            if (pending <= 0)
            {
                return false;
            }

            pending--;
            self.RefreshPlayerInfo();
            return true;
        }

        /// <summary>当前实际总值，有额外加成时括号显示；仅预分配时绿色 +N，无操作不显示 +0。</summary>
        private static void SetPrimaryAttributeStatText(Text text, NumericComponent numeric, int finalType, int pendingPoints)
        {
            if (text == null || numeric == null)
            {
                return;
            }

            int total = numeric.GetAsInt(finalType);
            int addKey = NumericType.ToComponentKey(finalType, NumericType.ComponentIndexAdd);
            int tempAddKey = NumericType.ToComponentKey(finalType, NumericType.ComponentIndexTempAdd);
            int extra = numeric.GetAsInt(addKey) + numeric.GetAsInt(tempAddKey);
            string bonusPart = extra != 0 ? $"({extra})" : string.Empty;

            if (pendingPoints <= 0)
            {
                text.text = $"{total}{bonusPart}";
                return;
            }

            text.supportRichText = true;
            text.text = $"{total}{bonusPart}+<color={PendingStatColor}>{pendingPoints}</color>";
        }

        private static void SetStatText(Text text, int value)
        {
            if (text == null)
            {
                return;
            }

            text.text = value.ToString();
        }

        private static void EnsureStatAdjustButtonsVisible(this UIEquipmentComponent self)
        {
            EnsureButtonVisible(self.m_btnStrLeft);
            EnsureButtonVisible(self.m_btnStrRight);
            EnsureButtonVisible(self.m_btnAgiLeft);
            EnsureButtonVisible(self.m_btnAgiRight);
            EnsureButtonVisible(self.m_btnVitLeft);
            EnsureButtonVisible(self.m_btnVitRight);
            EnsureButtonVisible(self.m_btnIntLeft);
            EnsureButtonVisible(self.m_btnIntRight);
            EnsureButtonVisible(self.m_btnDexLeft);
            EnsureButtonVisible(self.m_btnDexRight);
            EnsureButtonVisible(self.m_btnLukLeft);
            EnsureButtonVisible(self.m_btnLukRight);
        }

        private static void EnsureButtonVisible(Button button)
        {
            if (button != null && !button.gameObject.activeSelf)
            {
                button.gameObject.SetActive(true);
            }
        }

        private static void SetStatAdjustButtons(
            Button leftButton, Button rightButton, int pendingDelta, int nextCost, int allocatableRemaining)
        {
            if (leftButton != null)
            {
                EnsureButtonVisible(leftButton);
                leftButton.interactable = pendingDelta > 0;
            }

            if (rightButton != null)
            {
                EnsureButtonVisible(rightButton);
                rightButton.interactable = allocatableRemaining >= nextCost;
            }
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

        /// <summary>按当前六维公式显示派生战斗值（装备/Buff 走 Numeric Add）。</summary>
        private static void RefreshDerivedCombatDisplay(UIEquipmentComponent self, Unit unit, NumericComponent numeric)
        {
            UnitConfig unitConfig = unit?.Config();
            if (unitConfig == null || numeric == null)
            {
                return;
            }

            int level = numeric.GetAsInt(NumericType.Level);
            int str = numeric.GetAsInt(NumericType.STR);
            int agi = numeric.GetAsInt(NumericType.AGI);
            int vit = numeric.GetAsInt(NumericType.VIT);
            int intell = numeric.GetAsInt(NumericType.INT);
            int dex = numeric.GetAsInt(NumericType.DEX);
            int luk = numeric.GetAsInt(NumericType.LUK);

            int atkPrimary = unitConfig.Range > 1
                ? RoClassicPlayerCombatHelper.CalcPlayerRangedAtk(dex, str, luk)
                : RoClassicPlayerCombatHelper.CalcPlayerMeleeAtk(str, dex, luk);
            int atkEquipAdd = numeric.GetAsInt(NumericType.AtkAdd) + numeric.GetAsInt(NumericType.AtkTempAdd);
            int atkRandom = numeric.GetAsInt(NumericType.AtkRandom);
            SetCombatBreakdownStatText(self.m_textPhysicAttack, atkPrimary, atkEquipAdd, atkRandom);

            int defPrimary = RoClassicPlayerCombatHelper.CalcPlayerDef(vit);
            int defEquipAdd = numeric.GetAsInt(NumericType.DefAdd) + numeric.GetAsInt(NumericType.ToComponentKey(NumericType.Def, NumericType.ComponentIndexTempAdd));
            int defRandom = numeric.GetAsInt(NumericType.DefRandom);
            SetCombatBreakdownStatText(self.m_textPhysicDef, defPrimary, defEquipAdd, defRandom);

            int matkMin = RoClassicPlayerCombatHelper.CalcPlayerMAtkMin(intell);
            int matkEquipAdd = numeric.GetAsInt(NumericType.MAtkAdd) + numeric.GetAsInt(NumericType.ToComponentKey(NumericType.MAtk, NumericType.ComponentIndexTempAdd));
            int matkRandom = numeric.GetAsInt(NumericType.MAtkRandom);
            SetCombatBreakdownStatText(self.m_textMagicAttack, matkMin, matkEquipAdd, matkRandom);

            int mdefPrimary = RoClassicPlayerCombatHelper.CalcPlayerMDef(intell);
            int mdefEquipAdd = numeric.GetAsInt(NumericType.MDefAdd) + numeric.GetAsInt(NumericType.ToComponentKey(NumericType.MDef, NumericType.ComponentIndexTempAdd));
            int mdefRandom = numeric.GetAsInt(NumericType.MDefRandom);
            SetCombatBreakdownStatText(self.m_textMagicDef, mdefPrimary, mdefEquipAdd, mdefRandom);

            SetStatText(self.m_textHit, RoClassicPlayerCombatHelper.CalcPlayerHit(level, dex));
            SetStatText(self.m_textMiss, RoClassicPlayerCombatHelper.CalcPlayerFlee(level, agi));
        }

        private static void SetCombatBreakdownStatText(Text text, int baseVal, int equipAddSum, int random)
        {
            if (text == null)
            {
                return;
            }

            string bonusPart = equipAddSum != 0 ? $"({equipAddSum})" : string.Empty;
            if (random > 0)
            {
                text.text = $"{baseVal}{bonusPart}+0~{random}";
                return;
            }

            text.text = $"{baseVal}{bonusPart}";
        }

        public static void OnStrRight(this UIEquipmentComponent self) => self.AdjustPendingStat(0, true);
        public static void OnStrLeft(this UIEquipmentComponent self) => self.AdjustPendingStat(0, false);
        public static void OnAgiRight(this UIEquipmentComponent self) => self.AdjustPendingStat(1, true);
        public static void OnAgiLeft(this UIEquipmentComponent self) => self.AdjustPendingStat(1, false);
        public static void OnVitRight(this UIEquipmentComponent self) => self.AdjustPendingStat(2, true);
        public static void OnVitLeft(this UIEquipmentComponent self) => self.AdjustPendingStat(2, false);
        public static void OnIntRight(this UIEquipmentComponent self) => self.AdjustPendingStat(3, true);
        public static void OnIntLeft(this UIEquipmentComponent self) => self.AdjustPendingStat(3, false);
        public static void OnDexRight(this UIEquipmentComponent self) => self.AdjustPendingStat(4, true);
        public static void OnDexLeft(this UIEquipmentComponent self) => self.AdjustPendingStat(4, false);
        public static void OnLukRight(this UIEquipmentComponent self) => self.AdjustPendingStat(5, true);
        public static void OnLukLeft(this UIEquipmentComponent self) => self.AdjustPendingStat(5, false);

        /// <summary>仅修改界面预分配，不写角色数值；提交见 <see cref="RequestCloseAsync"/>。</summary>
        private static void AdjustPendingStat(this UIEquipmentComponent self, int statIndex, bool increase)
        {
            NumericComponent numeric = UnitHelper.GetMyUnitFromCurrentScene(self.Root()?.CurrentScene())
                ?.GetComponent<NumericComponent>();
            if (numeric == null)
            {
                return;
            }

            int str = numeric.GetAsInt(NumericType.STR);
            int agi = numeric.GetAsInt(NumericType.AGI);
            int vit = numeric.GetAsInt(NumericType.VIT);
            int intell = numeric.GetAsInt(NumericType.INT);
            int dex = numeric.GetAsInt(NumericType.DEX);
            int luk = numeric.GetAsInt(NumericType.LUK);
            int serverRemaining = numeric.GetAsInt(NumericType.RemainingAttributePoints);
            int allocatableRemaining = serverRemaining - ComputePendingPointsSpent(self, str, agi, vit, intell, dex, luk);

            switch (statIndex)
            {
                case 0:
                    if (increase) { self.TryIncreasePending(ref self.PendingStr, str, allocatableRemaining); }
                    else { self.TryDecreasePending(ref self.PendingStr); }
                    break;
                case 1:
                    if (increase) { self.TryIncreasePending(ref self.PendingAgi, agi, allocatableRemaining); }
                    else { self.TryDecreasePending(ref self.PendingAgi); }
                    break;
                case 2:
                    if (increase) { self.TryIncreasePending(ref self.PendingVit, vit, allocatableRemaining); }
                    else { self.TryDecreasePending(ref self.PendingVit); }
                    break;
                case 3:
                    if (increase) { self.TryIncreasePending(ref self.PendingInt, intell, allocatableRemaining); }
                    else { self.TryDecreasePending(ref self.PendingInt); }
                    break;
                case 4:
                    if (increase) { self.TryIncreasePending(ref self.PendingDex, dex, allocatableRemaining); }
                    else { self.TryDecreasePending(ref self.PendingDex); }
                    break;
                case 5:
                    if (increase) { self.TryIncreasePending(ref self.PendingLuk, luk, allocatableRemaining); }
                    else { self.TryDecreasePending(ref self.PendingLuk); }
                    break;
            }
        }
    }
}