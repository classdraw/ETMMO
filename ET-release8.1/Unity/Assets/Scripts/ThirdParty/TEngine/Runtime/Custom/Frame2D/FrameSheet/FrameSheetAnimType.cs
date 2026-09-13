namespace ET
{
    /// <summary>
    /// 2D 帧动画动作 Id，与全局 MotionType 数值一致。
    /// </summary>
    public static class FrameSheetAnimTypeId
    {
        public const int None = 0;
        public const int Idle = 1;
        public const int Stand = 2;
        public const int Move = 3;
        public const int Archery = 4;
        public const int Cast = 5;
        public const int Attack1 = 6;
        public const int Attack2 = 7;
        public const int Hit = 8;
        public const int Death = 9;
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
