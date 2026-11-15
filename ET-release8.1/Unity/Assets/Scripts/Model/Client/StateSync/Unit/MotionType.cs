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
        Hit = 3,
        Death01 = 4,
        Death02 = 5,
        
        
        Attack1 = 20,//攻击1
        Attack2 = 21,//攻击2
        Attack3 = 22,//攻击3
        Attack4 = 23,//攻击4
        AttackShield01 = 24,//盾牌攻击1
        AttackShield02 = 25,//盾牌攻击2
    }
}
