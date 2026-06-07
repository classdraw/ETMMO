namespace ET
{
    public enum SelectType
    {
        Self=0,//自身
        FriendlyTarget=1,//友方目标
        EnemyTarget=2,//敌方目标
        Position=3,//坐标 客户端传入
    }

    public enum ShapeType
    {
        //选的是啥就是啥，一个单位不做筛选 需要判断距离 合法性  0
        Single=0,
        //一个圈    1,1500,3,0
        //1500需要除以1000表示距离 |3表示人数|0 SelectCampType
        Circle=1,
        //需要通过参数判断正矩形还是斜的矩形  2,2000,3000,1,2,0
        //2000是长需要除以1000|3000是高需要除以1000|0表示正矩形，1表示斜的（施法者和目标点方向的矩形）|2表示人数|0 SelectCampType
        Rectangle=2,
        // 3,0,30000,3,0
        //0角色朝向的角度 1角色到目标点朝向角度 |30角度范围/1000f|3表示人数|0 SelectCampType
        Fan=3,//扇形
    }

    public enum SelectCampType
    {
        Ally=0,//盟友
        Hostile=1,//敌人
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

