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
        CampA=1,//玩家/友方统一阵营（安全区、常规图）
        CampB=2,//怪物阵营（常规图）
        CampPK=3,//自由PK模式：CampType 仅表示关系模式，实际敌友由 OwnerId/TeamId 判定
    }
    
    /// <summary>
    /// 队伍人数上限（自由PK小队伍）
    /// </summary>
    public static class TeamConst
    {
        public const int MaxMemberCount = 5;
    }
}

