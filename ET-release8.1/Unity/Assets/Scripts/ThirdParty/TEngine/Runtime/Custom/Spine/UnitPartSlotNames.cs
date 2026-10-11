namespace ET
{
    /// <summary>
    /// 部位槽位 Key 的中文说明（Inspector / 工具显示用）。
    /// 数值与 Model 中 UnitPartSlot 枚举一致。
    /// </summary>
    public static class UnitPartSlotNames
    {
        public static string GetChineseName(int partSlotKey)
        {
            switch (partSlotKey)
            {
                case 0: return "右眼·后";
                case 1: return "右眼·前";
                case 2: return "左眼·后";
                case 3: return "左眼·前";
                case 4: return "身体·头";
                case 5: return "身体·躯干";
                case 6: return "身体·左臂";
                case 7: return "身体·右臂";
                case 8: return "身体·左脚";
                case 9: return "身体·右脚";
                case 10: return "头发";
                case 11: return "胡须";
                case 12: return "衣服·左";
                case 13: return "衣服·右";
                case 14: return "衣服·身";
                case 15: return "裤子·右";
                case 16: return "裤子·左";
                case 17: return "头盔·后";
                case 18: return "头盔·前";
                case 19: return "铠甲·身";
                case 20: return "铠甲·左";
                case 21: return "铠甲·右";
                case 22: return "背饰";
                case 23: return "武器·左手·剑";
                case 24: return "武器·右手·剑";
                case 25: return "武器·左手·斧";
                case 26: return "武器·右手·斧";
                case 27: return "武器·左手·弓";
                case 28: return "武器·右手·弓";
                case 29: return "武器·左手·盾";
                case 30: return "武器·右手·盾";
                case 31: return "武器·左手·矛";
                case 32: return "武器·右手·矛";
                case 33: return "武器·左手·杖";
                case 34: return "武器·右手·杖";
                case 35: return "武器·左手·锤";
                case 36: return "武器·右手·锤";
                case 37: return "武器·左手·匕首";
                case 38: return "武器·右手·匕首";
                case 39: return "武器·左手·钉头锤";
                case 40: return "武器·右手·钉头锤";
                case 41: return "武器·左手·其他";
                case 42: return "武器·右手·其他";
                case 43: return "马·身体·后";
                case 44: return "马·身体·前";
                case 45: return "马·颈";
                case 46: return "马·头";
                case 47: return "马·前腿·前上";
                case 48: return "马·前腿·前下";
                case 49: return "马·前腿·后上";
                case 50: return "马·前腿·后下";
                case 51: return "马·后腿·前上";
                case 52: return "马·后腿·前下";
                case 53: return "马·后腿·后上";
                case 54: return "马·后腿·后下";
                case 55: return "马·尾";
                case 56: return "马·配饰";
                default: return "未知部位";
            }
        }
    }
}
