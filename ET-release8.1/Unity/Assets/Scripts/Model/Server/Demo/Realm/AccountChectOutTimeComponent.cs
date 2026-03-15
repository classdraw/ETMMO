namespace ET.Server
{
    [ComponentOf(typeof(Session))]
    public class AccountChectOutTimeComponent:Entity,IAwake<string>,IDestroy
    {
        public long Timer = 0;
        public string AccountName;
    }
}
