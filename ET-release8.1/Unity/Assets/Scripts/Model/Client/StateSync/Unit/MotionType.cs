namespace ET
{
    /// <summary>
    /// 全局动作类型（技能配置、3D Animator、2D 逻辑层共用）。
    /// </summary>
    public enum MotionType
    {
        None = 0,
        Idle = 1,//上下起伏idle
        Run = 2,//移动
        Attack1 = 3,//攻击1
        Attack2 = 4,//攻击2
        Hit = 5,
        Death01 = 6,
        Death02 = 7,
    }
}
