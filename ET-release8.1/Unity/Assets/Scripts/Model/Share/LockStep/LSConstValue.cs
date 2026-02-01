namespace ET
{
    public static class LSConstValue
    {
        public const int MatchCount = 1;//如果帧同步，这里是匹配数量 需要改成2~N
        public const int UpdateInterval = 50;
        public const int FrameCountPerSecond = 1000 / UpdateInterval;
        public const int SaveLSWorldFrameCount = 60 * FrameCountPerSecond;
    }
}