namespace ET
{
    /// <summary>
    /// 全局动作类型（技能配置、3D Animator、2D 逻辑层共用）。
    /// </summary>
    public enum MotionType
    {
        None = 0,
        Idle = 1,//上下起伏idle
        Stand = 2,//站立不动
        Move = 3,//移动
        Archery = 4,//射箭
        Cast = 5,//施法
        Attack1 = 6,//攻击1
        Attack2 = 7,//攻击2
        Hit = 8,
        Death = 9
    }
}
