namespace ET.Server
{
    [FriendOf(typeof(NumericComponent))]
    public static class PlayerAttributeHelper
    {
        public static bool TryCommitAttributeDeltas(
            Unit unit,
            int deltaStr,
            int deltaAgi,
            int deltaVit,
            int deltaInt,
            int deltaDex,
            int deltaLuk,
            out string message)
        {
            message = null;
            NumericComponent numeric = unit?.GetComponent<NumericComponent>();
            if (numeric == null)
            {
                message = "NumericComponent 为空";
                return false;
            }

            if (deltaStr < 0 || deltaAgi < 0 || deltaVit < 0 || deltaInt < 0 || deltaDex < 0 || deltaLuk < 0)
            {
                message = "属性增量不能为负";
                return false;
            }

            if (deltaStr + deltaAgi + deltaVit + deltaInt + deltaDex + deltaLuk <= 0)
            {
                message = "未分配任何属性点";
                return false;
            }

            int str = numeric.GetPrimaryStatBase(NumericType.STR);
            int agi = numeric.GetPrimaryStatBase(NumericType.AGI);
            int vit = numeric.GetPrimaryStatBase(NumericType.VIT);
            int intell = numeric.GetPrimaryStatBase(NumericType.INT);
            int dex = numeric.GetPrimaryStatBase(NumericType.DEX);
            int luk = numeric.GetPrimaryStatBase(NumericType.LUK);

            int maxStat = RoClassicLevelHelper.GetMaxPrimaryStatValue();
            if (str + deltaStr > maxStat || agi + deltaAgi > maxStat || vit + deltaVit > maxStat ||
                intell + deltaInt > maxStat || dex + deltaDex > maxStat || luk + deltaLuk > maxStat)
            {
                message = $"单项属性不能超过 {maxStat}";
                return false;
            }

            int totalCost = 0;
            totalCost += RoClassicLevelHelper.ComputeAttributePointCostForIncreaseRange(str, deltaStr);
            totalCost += RoClassicLevelHelper.ComputeAttributePointCostForIncreaseRange(agi, deltaAgi);
            totalCost += RoClassicLevelHelper.ComputeAttributePointCostForIncreaseRange(vit, deltaVit);
            totalCost += RoClassicLevelHelper.ComputeAttributePointCostForIncreaseRange(intell, deltaInt);
            totalCost += RoClassicLevelHelper.ComputeAttributePointCostForIncreaseRange(dex, deltaDex);
            totalCost += RoClassicLevelHelper.ComputeAttributePointCostForIncreaseRange(luk, deltaLuk);

            int remaining = numeric.GetAsInt(NumericType.RemainingAttributePoints);
            if (totalCost > remaining)
            {
                message = $"属性点不足，需要 {totalCost}，剩余 {remaining}";
                return false;
            }

            if (deltaStr > 0)
            {
                numeric.Set(NumericType.STRBase, str + deltaStr);
            }

            if (deltaAgi > 0)
            {
                numeric.Set(NumericType.AGIBase, agi + deltaAgi);
            }

            if (deltaVit > 0)
            {
                numeric.Set(NumericType.VITBase, vit + deltaVit);
            }

            if (deltaInt > 0)
            {
                numeric.Set(NumericType.INTBase, intell + deltaInt);
            }

            if (deltaDex > 0)
            {
                numeric.Set(NumericType.DEXBase, dex + deltaDex);
            }

            if (deltaLuk > 0)
            {
                numeric.Set(NumericType.LUKBase, luk + deltaLuk);
            }

            numeric.Set(NumericType.RemainingAttributePoints, remaining - totalCost);
            RefreshPrimaryDerivedCombatStats(unit, numeric);

            message = $"已分配属性点 {totalCost}，剩余 {numeric.GetAsInt(NumericType.RemainingAttributePoints)}";
            return true;
        }

        /// <summary>按当前六维与等级重算 HP/SP 与攻/防/命中/闪避等（创角、升级、加点后需调用）。</summary>
        public static void RefreshPrimaryDerivedCombatStats(Unit unit, NumericComponent numeric)
        {
            UnitConfig unitConfig = unit.Config();
            if (unitConfig == null)
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

            int hp = NumericHelper.CalcHpResult(level, vit, unitConfig.JobHp / 1000f);
            int sp = NumericHelper.CalcSpResult(level, intell, unitConfig.JobSp / 1000f);
            numeric.Set(NumericType.MaxHpBase, hp);
            numeric.Set(NumericType.MaxSpBase, sp);

            bool isRanged = unitConfig.Range > 1;
            int atk = isRanged
                ? NumericHelper.CalcPlayerRangedAtk(dex, str, luk)
                : NumericHelper.CalcPlayerMeleeAtk(str, dex, luk);
            numeric.Set(NumericType.AtkBase, atk);
            numeric.Set(NumericType.AtkRandom, 0);
            numeric.Set(NumericType.DefBase, NumericHelper.CalcPlayerDef(vit));
            numeric.Set(NumericType.DefRandom, 0);

            int matkMin = NumericHelper.CalcPlayerMAtkMin(intell);
            int matkMax = NumericHelper.CalcPlayerMAtkMax(intell);
            numeric.Set(NumericType.MAtkBase, matkMin);
            numeric.Set(NumericType.MAtkRandom, matkMax - matkMin);
            numeric.Set(NumericType.MDefBase, NumericHelper.CalcPlayerMDef(intell));
            numeric.Set(NumericType.MDefRandom, 0);
            numeric.Set(NumericType.Hit, NumericHelper.CalcPlayerHit(level, dex));
            numeric.Set(NumericType.Flee, NumericHelper.CalcPlayerFlee(level, agi));
            numeric.Set(NumericType.AtkSpeed, NumericHelper.CalcPlayerAtkSpeed(agi, dex));
        }
    }
}