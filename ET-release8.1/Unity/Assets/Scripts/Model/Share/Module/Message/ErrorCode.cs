namespace ET
{
    public static partial class ErrorCode
    {
        public const int ERR_Success = 0;

        // 1-11004 是SocketError请看SocketError定义
        //-----------------------------------
        // 100000-109999是Core层的错误
        
        // 110000以下的错误请看ErrorCore.cs
        
        // 这里配置逻辑层的错误码
        // 110000 - 200000是抛异常的错误
        // 200001以上不抛异常
        public const int ERR_LoginInfoEmpty = 200002;//旧的废弃
        public const int ERR_LoginPwdError = 200003;//旧的废弃
        public const int ERR_RequestRepeatedly = 200004;//session重复请求
        public const int ERR_LoginInfoNull = 200005;//登录输入是null
        public const int ERR_AccountFormError = 200006;//账号正则错误
        public const int ERR_LoginPasswordError = 200007;//账号密码错误
        public const int ERR_AccountInBlackListError = 200008;//黑名单
        public const int ERR_TokenError = 200009;//token异常
        public const int ERR_RoleNameNull = 200010;//创建角色null 
        public const int ERR_RoleNameSame = 200011;//创建角色same 
        public const int ERR_RoleNotExist = 200012;//角色不存在
        public const int ERR_LoginGameGateError01 = 200013;//gateInstanceId不一致
        
    }
}