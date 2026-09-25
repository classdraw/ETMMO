namespace ET
{
    public static class ActionsType
    {
        public const int NumericChange=1;//改变目标数值，如果是buff，删除会还原数值
        public const int Damage = 2;//伤害行为
        public const int CastBullet = 3;//创建子弹
        public const int CastEmptyBullet = 4;//假子弹逻辑
        public const int MoveToTarget = 5;//往目标移动
        public const int CreateCast = 6;//释放一个新的Cast
        public const int Attract = 7;//把目标往中心吸引多少米
        public const int HitFlyTarget = 8;//击飞距离多少米，并给目标增加debuff
    }
}

