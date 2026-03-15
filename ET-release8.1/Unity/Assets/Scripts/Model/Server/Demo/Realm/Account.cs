using System;

namespace ET.Server
{
    public enum AccountType
    {
        General=0,//一般
        BlackList=9//黑名单
    }

    //账号类
    [ChildOf(typeof(Session))]
    public class Account:Entity,IAwake
    {
        public string AccountName;
        public string Password;
        public int AccountType;//0正常 1黑名单 等等
        public long CreateTime;//建号时间
        public long LastLoginTime;//最后一次登录
    }
    
}