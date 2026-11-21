namespace ET.Server
{
    [FriendOf(typeof(NumericComponent))]
    public static class PlayerLevelHelper
    {
        /// <summary>创角：按当前 <see cref="NumericType.Level"/>（或传入等级）写入经验条与未消耗点。</summary>
        public static void InitLevelProgress(NumericComponent numeric, int level = 1)
        {
            if (numeric == null)
            {
                return;
            }

            if (level < 1)
            {
                level = 1;
            }

            numeric.Set(NumericType.RemainingAttributePoints, RoClassicLevelHelper.GetTotalRemainingAttributePointsForLevel(level));
            numeric.Set(NumericType.RemainingSkillPoints, RoClassicLevelHelper.GetTotalRemainingSkillPointsForLevel(level));
            numeric.Set(NumericType.EXP, 0);
        }

        /// <summary>读档补缺：NumericDic 中缺 1100~1102 时按等级补默认（不覆盖已有值）。</summary>
        public static bool EnsureLevelProgressPersisted(NumericComponent numeric)
        {
            if (numeric?.NumericDic == null)
            {
                return false;
            }

            int level = numeric.GetAsInt(NumericType.Level);
            if (level < 1)
            {
                level = 1;
            }

            bool changed = false;

            if (!numeric.NumericDic.ContainsKey(NumericType.RemainingAttributePoints))
            {
                numeric.SetNoEvent(NumericType.RemainingAttributePoints,
                    RoClassicLevelHelper.GetTotalRemainingAttributePointsForLevel(level));
                changed = true;
            }

            if (!numeric.NumericDic.ContainsKey(NumericType.RemainingSkillPoints))
            {
                numeric.SetNoEvent(NumericType.RemainingSkillPoints,
                    RoClassicLevelHelper.GetTotalRemainingSkillPointsForLevel(level));
                changed = true;
            }

            if (!numeric.NumericDic.ContainsKey(NumericType.EXP))
            {
                numeric.SetNoEvent(NumericType.EXP, 0);
                changed = true;
            }

            return changed;
        }

        public static int AddBaseExp(Unit unit, long amount)
        {
            if (unit == null || unit.IsDisposed || amount <= 0)
            {
                return 0;
            }

            NumericComponent numeric = unit.GetComponent<NumericComponent>();
            if (numeric == null)
            {
                return 0;
            }

            long exp = numeric[NumericType.EXP] + amount;
            if (exp > int.MaxValue)
            {
                exp = int.MaxValue;
            }

            numeric.Set(NumericType.EXP, (int)exp);
            return ProcessLevelUps(unit, numeric);
        }

        /// <summary>GM：直接升 1 级（不扣经验），已满级返回 0。</summary>
        public static int GmLevelUpOnce(Unit unit)
        {
            NumericComponent numeric = unit?.GetComponent<NumericComponent>();
            if (numeric == null)
            {
                return 0;
            }

            int level = numeric.GetAsInt(NumericType.Level);
            if (!RoClassicLevelHelper.CanLevelUp(level))
            {
                return 0;
            }

            ApplyLevelUp(unit, numeric, level + 1);
            return 1;
        }

        private static int ProcessLevelUps(Unit unit, NumericComponent numeric)
        {
            int leveled = 0;
            while (true)
            {
                int level = numeric.GetAsInt(NumericType.Level);
                if (!RoClassicLevelHelper.CanLevelUp(level))
                {
                    break;
                }

                int need = RoClassicLevelHelper.GetNeedExpToNext(level);
                long exp = numeric[NumericType.EXP];
                if (exp < need)
                {
                    break;
                }

                numeric.Set(NumericType.EXP, (int)(exp - need));
                ApplyLevelUp(unit, numeric, level + 1);
                leveled++;
            }

            return leveled;
        }

        private static void ApplyLevelUp(Unit unit, NumericComponent numeric, int newLevel)
        {
            numeric.Set(NumericType.Level, newLevel);

            int addStat = RoClassicLevelHelper.GetStatPointsOnReachLevel(newLevel);
            if (addStat > 0)
            {
                numeric.Set(NumericType.RemainingAttributePoints,
                    numeric.GetAsInt(NumericType.RemainingAttributePoints) + addStat);
            }

            int addSkill = RoClassicLevelHelper.GetSkillPointsOnReachLevel(newLevel);
            if (addSkill > 0)
            {
                numeric.Set(NumericType.RemainingSkillPoints,
                    numeric.GetAsInt(NumericType.RemainingSkillPoints) + addSkill);
            }

            RefreshPlayerLevelDerived(unit, numeric, newLevel);
        }

        private static void RefreshPlayerLevelDerived(Unit unit, NumericComponent numeric, int level)
        {
            UnitConfig unitConfig = unit.Config();
            int vit = numeric.GetAsInt(NumericType.VIT);
            int intell = numeric.GetAsInt(NumericType.INT);
            int agi = numeric.GetAsInt(NumericType.AGI);
            int dex = numeric.GetAsInt(NumericType.DEX);

            int hp = NumericHelper.CalcHpResult(level, vit, unitConfig.JobHp / 1000f);
            int sp = NumericHelper.CalcSpResult(level, intell, unitConfig.JobSp / 1000f);
            numeric.Set(NumericType.MaxHpBase, hp);
            numeric.Set(NumericType.MaxSpBase, sp);

            numeric.Set(NumericType.Hit, NumericHelper.CalcPlayerHit(level, dex));
            numeric.Set(NumericType.Flee, NumericHelper.CalcPlayerFlee(level, agi));
        }
    }
}