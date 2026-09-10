namespace ET
{
    /// <summary>
    /// 动作类型（2D 帧动画、3D Animator 与技能配置共用）。
    /// </summary>
    public enum MotionType
    {
        None = 0,
        Idle = 1,//上下起伏idle
        Stand = 2,//站立不动
        Move=3,//移动
        Archery=4,//射箭
        Cast=5,//施法
        Attack1=6,//攻击1
        Attack2=7,//攻击2
        Hit=8,
        Death=9
    }

    /// <summary>
    /// 帧动画四方向。
    /// </summary>
    public enum FrameSheetFacing
    {
        Down = 0,
        Left = 1,
        Right = 2,
        Up = 3,
    }
}
