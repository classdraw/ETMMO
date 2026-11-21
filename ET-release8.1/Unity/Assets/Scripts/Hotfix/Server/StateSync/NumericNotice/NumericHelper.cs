using System.Collections.Generic;
using Unity.Mathematics;
using System;

namespace ET.Server
{
    [FriendOf(typeof(NumericComponent))]
    public static partial class NumericHelper
    {
        public static int SumFromOneToN(int n) => RoClassicPlayerCombatHelper.SumFromOneToN(n);

        public static int CalcHpResult(int level, int vit, double jobHp) => RoClassicPlayerCombatHelper.CalcHpResult(level, vit, jobHp);

        public static int CalcSpResult(int level, int intell, double jobSp) => RoClassicPlayerCombatHelper.CalcSpResult(level, intell, jobSp);

        public static int CalcPlayerMeleeAtk(int str, int dex, int luk) => RoClassicPlayerCombatHelper.CalcPlayerMeleeAtk(str, dex, luk);

        public static int CalcPlayerRangedAtk(int dex, int str, int luk) => RoClassicPlayerCombatHelper.CalcPlayerRangedAtk(dex, str, luk);

        public static int CalcPlayerDef(int vit) => RoClassicPlayerCombatHelper.CalcPlayerDef(vit);

        public static int CalcPlayerMAtkMin(int intell) => RoClassicPlayerCombatHelper.CalcPlayerMAtkMin(intell);

        public static int CalcPlayerMAtkMax(int intell) => RoClassicPlayerCombatHelper.CalcPlayerMAtkMax(intell);

        public static int CalcPlayerMDef(int intell) => RoClassicPlayerCombatHelper.CalcPlayerMDef(intell);

        public static int CalcPlayerHit(int level, int dex) => RoClassicPlayerCombatHelper.CalcPlayerHit(level, dex);

        public static int CalcPlayerFlee(int level, int agi) => RoClassicPlayerCombatHelper.CalcPlayerFlee(level, agi);

        public static float CalcPlayerAtkSpeed(int agi, int dex) => RoClassicPlayerCombatHelper.CalcPlayerAtkSpeed(agi, dex);
    }
}

