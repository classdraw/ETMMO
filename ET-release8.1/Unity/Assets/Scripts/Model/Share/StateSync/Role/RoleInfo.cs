
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
        public long LastLoginTime;//离线保存的时候做处理
        public long CreateTime;
        public int ServerId;
        
        
        /// <summary>创角档案：Race,Gender,Job（枚举 int，0=None，逗号分隔）。协议字段名仍为 BaseExternalDisplay。</summary>
        public string BaseExternalDisplay;

    }
}
