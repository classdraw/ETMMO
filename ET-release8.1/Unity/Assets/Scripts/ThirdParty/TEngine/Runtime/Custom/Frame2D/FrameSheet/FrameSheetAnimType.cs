namespace ET
{
    /// <summary>
    /// 帧动画名枚举（与 AvatarType、Animator 等区分）。
    /// </summary>
    public enum FrameSheetAnimType
    {
        None = 0,
        Idle = 1,//上下起伏idle
        Stand = 2,//战力不动
        Move=3,//移动
        Archery=5,//射箭
        Cast=6,//施法
    }

    /// <summary>
    /// 帧动画四方向（与 FrameSheetAnimType 区分）。
    /// </summary>
    public enum FrameSheetFacing
    {
        Down = 0,
        Left = 1,
        Right = 2,
        Up = 3,
    }
}
