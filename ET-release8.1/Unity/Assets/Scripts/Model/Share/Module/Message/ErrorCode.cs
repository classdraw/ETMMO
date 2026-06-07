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
        //角色创建
        public const int ERR_RoleNameNull = 200010;//创建角色null 
        public const int ERR_RoleNameSame = 200011;//创建角色same 
        public const int ERR_RoleNotExist = 200012;//角色不存在
        public const int ERR_LoginGameGateError01 = 200013;//gateInstanceId不一致
        //登录角色
        public const int ERR_SessionPlayerError = 200014;//SessionPlayer组件丢失 没有正常登陆  没有走正常的loginGameGate逻辑
        public const int ERR_NonePlayerError = 200015;//玩家对象丢失 或者释放  和200014一样问题
        public const int ERR_PlayerSessionError = 200016;//玩家session释放
        public const int ERR_RepeatedEnterGameError1 = 200017;//玩家重复登陆失败
        public const int ERR_RepeatedEnterGameError2 = 200018;//玩家进入游戏逻辑服异常
        public const int ERR_ErrorEnterGame = 200019;//玩家进入游戏逻辑服异常
        public const int ERR_AddKnapsackItemError = 200020;//背包增加物品异常
        public const int ERR_RemoveKnapsackItemError = 200021;//背包移除物品异常
        public const int ERR_NetWorkError = 200022;//一些网络异常
        //邮箱
        public const int ERR_MailNotExist = 200023; //邮件不存在
        public const int ERR_MailCollected = 200024; //邮件已领取
        public const int ERR_MailConfigNotExist = 200025; //邮件配置不存在

        //队伍
        public const int ERR_TeamNameNull = 200026; //队伍名为空
        public const int ERR_TeamAlreadyExist = 200027; //已在队伍中
        public const int ERR_TeamNotExist = 200028; //不在队伍中
        public const int ERR_TeamNotInTeam = 200029; //不是该队伍成员
        public const int ERR_TeamNotLeader = 200030; //不是队长
        
        //技能
        public const int ERR_CastSkillError = 200050;//释放技能失败
        public const int ERR_CastArgsError = 200051;//释放技能参数异常
        public const int ERR_CastCasterIsNullError = 200052;//释放者为空
        public const int ERR_CastConfigError = 200053;//释放技能配置错误
        public const int ERR_CastInputUnitError = 200054;//释放技能目标错误
        
        //地图
        public const int ERR_EnterMapError = 200100;//地图进入错误
        
        public const int ERR_None = 300000;//占位
    }
}