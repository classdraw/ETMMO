using ET.Server;

namespace ET
{
    public enum RoleInfoState
    {
        Normal=0,
        Freeze,
    }

    //角色
    [ChildOf]
    public class RoleInfo:Entity,IAwake
    {
        public string Name;
        public int State;
        public string AccountName;
        public long LastLoginTime;
        public long CreateTime;
        public int ServerId;
        
    }
}
