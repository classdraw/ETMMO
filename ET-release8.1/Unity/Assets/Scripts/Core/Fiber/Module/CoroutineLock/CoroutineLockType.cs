namespace ET
{
    public static class CoroutineLockType
    {
        public const int None = 0;
        public const int Location = 1;                  // location进程上使用
        public const int MessageLocationSender = 2;       // MessageLocationSender中队列消息 
        public const int Mailbox = 3;                   // Mailbox中队列
        public const int UnitId = 4;                    // Map服务器上线下线时使用
        public const int DB = 5;
        public const int Resources = 6;
        public const int ResourcesLoader = 7;

        public const int LoginAccount = 8;//登录携程锁
        public const int CreateRole = 9;//创建角色
        public const int LoginCenterLock = 10;//登录服锁住
        public const int LoginGate = 11;//登陆gate网关服务器
        public const int UnitCacheGet = 12;//缓存服务器锁

        public const int Max = 100; // 这个必须最大
    }
}