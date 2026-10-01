namespace ET
{
	// 这个可弄个配置表生成
    public static class NumericType
    {
	    public const int Max = 10000;

	    /// <summary>Final 与子项键间隔：子项键 = Final * ComponentMultiplier + 偏移(1~9)。</summary>
	    public const int ComponentMultiplier = 100;

	    /// <summary>子项：Base=+1, Add=+2, Pct=+3, FinalAdd=+4, FinalPct=+5；临时=+6~+9，结算时与 +2~+5 相加。</summary>
	    public const int ComponentIndexBase = 1;
	    public const int ComponentIndexAdd = 2;
	    public const int ComponentIndexPct = 3;
	    public const int ComponentIndexFinalAdd = 4;
	    public const int ComponentIndexFinalPct = 5;
	    public const int ComponentIndexTempAdd = 6;
	    public const int ComponentIndexTempPct = 7;
	    public const int ComponentIndexTempFinalAdd = 8;
	    public const int ComponentIndexTempFinalPct = 9;

	    public static int ToComponentKey(int final, int componentIndex) => final * ComponentMultiplier + componentIndex;

	    public static int ToFinalNumericType(int numericType)
	    {
		    if (!IsComponentKey(numericType))
		    {
			    return numericType;
		    }

		    return numericType / ComponentMultiplier;
	    }

	    public static bool IsComponentKey(int numericType)
	    {
		    if (numericType < Max)
		    {
			    return false;
		    }

		    int index = numericType % ComponentMultiplier;
		    return index >= ComponentIndexBase && index <= ComponentIndexTempFinalPct;
	    }

	    public static bool IsTempComponentKey(int numericType)
	    {
		    if (!IsComponentKey(numericType))
		    {
			    return false;
		    }

		    int index = numericType % ComponentMultiplier;
		    return index >= ComponentIndexTempAdd && index <= ComponentIndexTempFinalPct;
	    }

	    public const int Speed = 1000;//速度
	    public const int SpeedBase = Speed * ComponentMultiplier + 1;
	    public const int SpeedAdd = Speed * ComponentMultiplier + 2;
	    public const int SpeedTempAdd = Speed * ComponentMultiplier + 6;
	    //public const int SpeedPct = Speed * ComponentMultiplier + 3;
	    //public const int SpeedTempPct = Speed * ComponentMultiplier + 7;
	    //public const int SpeedFinalAdd = Speed * ComponentMultiplier + 4;
	    //public const int SpeedTempFinalAdd = Speed * ComponentMultiplier + 8;
	    //public const int SpeedFinalPct = Speed * ComponentMultiplier + 5;
	    //public const int SpeedTempFinalPct = Speed * ComponentMultiplier + 9;

	    public const int Hp = 1001;//血量
	    public const int HpBase = Hp * ComponentMultiplier + 1;


	    public const int MaxHp = 1002;
	    public const int MaxHpBase = MaxHp * ComponentMultiplier + 1;
	    public const int MaxHpAdd = MaxHp * ComponentMultiplier + 2;
	    public const int MaxHpTempAdd = MaxHp * ComponentMultiplier + 6;
	    public const int MaxHpPct = MaxHp * ComponentMultiplier + 3;
	    public const int MaxHpTempPct = MaxHp * ComponentMultiplier + 7;
	    public const int MaxHpFinalAdd = MaxHp * ComponentMultiplier + 4;
	    public const int MaxHpTempFinalAdd = MaxHp * ComponentMultiplier + 8;
	    public const int MaxHpFinalPct = MaxHp * ComponentMultiplier + 5;
	    public const int MaxHpTempFinalPct = MaxHp * ComponentMultiplier + 9;

	    public const int AOI = 1003;//aoi范围
	    //public const int AOIBase = AOI * ComponentMultiplier + 1;
	    //public const int AOIAdd = AOI * ComponentMultiplier + 2;
	    //public const int AOIPct = AOI * ComponentMultiplier + 3;
	    //public const int AOIFinalAdd = AOI * ComponentMultiplier + 4;
	    //public const int AOIFinalPct = AOI * ComponentMultiplier + 5;

	    public const int Level = 1004;//等级
	    
	    //常规属性
	    public const int STR = 1005;//力量
	    public const int STRBase = STR * ComponentMultiplier + 1;
	    public const int STRAdd = STR * ComponentMultiplier + 2;

	    public const int AGI = 1006;//敏捷
	    public const int AGIBase = AGI * ComponentMultiplier + 1;
	    public const int AGIAdd = AGI * ComponentMultiplier + 2;
	    
	    public const int VIT = 1007;//体质
	    public const int VITBase = VIT * ComponentMultiplier + 1;
	    public const int VITAdd = VIT * ComponentMultiplier + 2;
	    
	    
	    public const int INT = 1008;//智力
	    public const int INTBase = INT * ComponentMultiplier + 1;
	    public const int INTAdd = INT * ComponentMultiplier + 2;

	    
	    public const int DEX = 1009;//灵巧
	    public const int DEXBase = DEX * ComponentMultiplier + 1;
	    public const int DEXAdd = DEX * ComponentMultiplier + 2;
	    
	    public const int LUK = 1010;//幸运
	    public const int LUKBase = LUK * ComponentMultiplier + 1;
	    public const int LUKAdd = LUK * ComponentMultiplier + 2;
	    
	    public const int Sp = 1011;//蓝量
	    public const int SpBase = Sp * ComponentMultiplier + 1;

	    public const int Element = 1012;//元素
	    //预留1012 1019
	    
	    //战斗属性
	    public const int Atk = 1020;//攻击力
	    public const int AtkBase = Atk * ComponentMultiplier + 1;
	    public const int AtkAdd = Atk * ComponentMultiplier + 2;
	    public const int AtkTempAdd = Atk * ComponentMultiplier + 6;
	    
	    public const int Def = 1021;//物理防御
	    public const int DefBase = Def * ComponentMultiplier + 1;
	    public const int DefAdd = Def * ComponentMultiplier + 2;
	    
	    public const int MAtk = 1022;//魔法攻击力
	    public const int MAtkBase = MAtk * ComponentMultiplier + 1;
	    public const int MAtkAdd = MAtk * ComponentMultiplier + 2;
	    
	    public const int MDef = 1023;//魔法防御
	    public const int MDefBase = MDef * ComponentMultiplier + 1;
	    public const int MDefAdd = MDef * ComponentMultiplier + 2;

	    public const int Hit = 1024;//命中
	    public const int Flee = 1025;//95% miss值

	    public const int AtkSpeed = 1026;//攻速
	    public const int AtkRange = 1027;//普攻距离


	    public const int AtkRandom = 1028;//物理攻击随机浮动
	    public const int DefRandom = 1029;//物理防御随机浮动
	    public const int MAtkRandom = 1030;//魔法攻击随机浮动
	    public const int MDefRandom = 1031;//魔法防御随机浮动


	    public const int ForbidSkill = 1050;//禁止施法技能状态
	    public const int ForbidMove = 1051;//禁止移动
	    public const int ForbidRotation = 1052;//禁止朝向 只是禁用移动转向
    }
}
