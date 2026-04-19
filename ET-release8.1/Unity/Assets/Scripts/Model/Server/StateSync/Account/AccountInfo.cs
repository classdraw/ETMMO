namespace ET.Server
{
    //账号类
    [ChildOf(typeof(AccountInfoComponent))]
    public class AccountInfo:Entity,IAwake
    {
        public string Account;
        public string Password;
    }
    
}

