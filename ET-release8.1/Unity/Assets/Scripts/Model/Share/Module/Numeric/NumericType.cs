namespace ET
{
	// 这个可弄个配置表生成
    public static class NumericType
    {
	    public const int Max = 10000;

	    public const int Speed = 1000;//速度
	    public const int SpeedBase = Speed * 10 + 1;
	    public const int SpeedAdd = Speed * 10 + 2;
	    //public const int SpeedPct = Speed * 10 + 3;
	    //public const int SpeedFinalAdd = Speed * 10 + 4;
	    //public const int SpeedFinalPct = Speed * 10 + 5;

	    public const int Hp = 1001;//血量
	    public const int HpBase = Hp * 10 + 1;


	    public const int MaxHp = 1002;
	    public const int MaxHpBase = MaxHp * 10 + 1;
	    public const int MaxHpAdd = MaxHp * 10 + 2;
	    public const int MaxHpPct = MaxHp * 10 + 3;
	    public const int MaxHpFinalAdd = MaxHp * 10 + 4;
	    public const int MaxHpFinalPct = MaxHp * 10 + 5;

	    public const int AOI = 1003;//aoi范围
	    //public const int AOIBase = AOI * 10 + 1;
	    //public const int AOIAdd = AOI * 10 + 2;
	    //public const int AOIPct = AOI * 10 + 3;
	    //public const int AOIFinalAdd = AOI * 10 + 4;
	    //public const int AOIFinalPct = AOI * 10 + 5;

	    public const int Level = 1004;//等级
	    
	    //常规属性
	    public const int STR = 1005;//力量
	    public const int STRBase = STR * 10 + 1;
	    public const int STRAdd = STR * 10 + 2;

	    public const int AGI = 1006;//敏捷
	    public const int AGIBase = AGI * 10 + 1;
	    public const int AGIAdd = AGI * 10 + 2;
	    
	    public const int VIT = 1007;//体质
	    public const int VITBase = VIT * 10 + 1;
	    public const int VITAdd = VIT * 10 + 2;
	    
	    
	    public const int INT = 1008;//智力
	    public const int INTBase = INT * 10 + 1;
	    public const int INTAdd = INT * 10 + 2;

	    
	    public const int DEX = 1009;//灵巧
	    public const int DEXBase = DEX * 10 + 1;
	    public const int DEXAdd = DEX * 10 + 2;
	    
	    public const int LUK = 1010;//幸运
	    public const int LUKBase = LUK * 10 + 1;
	    public const int LUKAdd = LUK * 10 + 2;
	    
	    public const int Sp = 1011;//蓝量
	    public const int SpBase = Sp * 10 + 1;

	    public const int Element = 1012;//元素
	    //预留1012 1019
	    
	    //战斗属性
	    public const int Atk = 1020;//攻击力
	    public const int AtkBase = Atk * 10 + 1;
	    public const int AtkAdd = Atk * 10 + 2;
	    
	    public const int Def = 1021;//物理防御
	    public const int DefBase = Def * 10 + 1;
	    public const int DefAdd = Def * 10 + 2;
	    
	    public const int MAtk = 1022;//魔法攻击力
	    public const int MAtkBase = MAtk * 10 + 1;
	    public const int MAtkAdd = MAtk * 10 + 2;
	    
	    public const int MDef = 1023;//魔法防御
	    public const int MDefBase = MDef * 10 + 1;
	    public const int MDefAdd = MDef * 10 + 2;

	    public const int Hit = 1024;//命中
	    public const int Flee = 1025;//95% miss值

	    public const int AtkSpeed = 1026;//攻速
	    public const int AtkRange = 1027;//普攻距离


	    public const int AtkRandom = 1028;//物理攻击随机浮动
	    public const int DefRandom = 1029;//物理防御随机浮动
	    public const int MAtkRandom = 1030;//魔法攻击随机浮动
	    public const int MDefRandom = 1031;//魔法防御随机浮动
	    
    }
}
