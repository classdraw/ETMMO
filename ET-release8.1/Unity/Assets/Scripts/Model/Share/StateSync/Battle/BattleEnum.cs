namespace ET
{
    public enum SelectType
    {
        None=0,
        Self=1,//自身
        FriendlyTarget=2,//友方目标
        EnemyTarget=3,//敌方目标
        Position=9,//坐标 客户端传入
    }

        
    public enum CampType
    {
        None=0,
        CampA=1,//玩家阵营
        CampB=2,//怪物阵营
        CampPK=3//自由攻击模式就用到
    }
}

