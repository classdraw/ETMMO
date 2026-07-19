namespace ET
{
    public static class ActionsType
    {
        public const int NumericChange=1;//改变目标数值，如果是buff，删除会还原数值
        public const int Damage = 2;//伤害行为
        public const int CastBullet = 3;//创建子弹
        public const int CastEmptyBullet = 4;//假子弹逻辑
    }
}

