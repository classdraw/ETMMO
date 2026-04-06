
namespace ET
{
    public enum RoleInfoState
    {
        Normal=0,//正常状态
        Freeze=1,//冻结状态
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

        /// <summary>最近一次在地图服触发的下线/持久化时间（秒，服务器时间）。</summary>
        public long LastOfflineTime;
        
    }
}
