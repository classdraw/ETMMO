namespace ET
{
    [UniqueId(100, 10000)]
    public static class TimerInvokeType
    {
        // 框架层100-200，逻辑层的timer type从200起
        public const int WaitTimer = 100;
        public const int SessionIdleChecker = 101;
        public const int MessageLocationSenderChecker = 102;
        public const int MessageSenderChecker = 103;
        
        // 框架层100-200，逻辑层的timer type 200-300
        public const int MoveTimer = 201;
        public const int AITimer = 202;
        public const int SessionAcceptTimeout = 203;
        
        public const int RoomUpdate = 301;
        public const int MapCloseCheckTimer = 302;//地图是否关闭检测
        
        public const int PlayerOfflineOutTimer = 401;//玩家超时
        public const int SaveChangeDBDateTimer = 402;
        
        public const int NumericSyncTimer = 403;//数值同步
        public const int AccountChectOutTimer = 404;//账号超时

        public const int BuffExpireTimer = 405;//buff超时处理
        public const int BuffTickTimer = 406;//bufftick处理
        public const int BulletTickTimer = 407;//bullet定时器
        public const int BulletExpireTimer = 408;//bullet超时销毁

    }
}