namespace ET.Server
{
    public enum PlayerState
    {   
        Disconnect,//断开状态
        Gate,//和网关服链接 没有进入map服务器
        Game//进入游戏状态 在map服务器
    }

    //登陆玩家实体
    [ChildOf(typeof(PlayerComponent))]
    public sealed class Player : Entity, IAwake<string,int,string>//id就是 数据库roleId
    {
        public long UnitId { get; set; }//数据库id 也是唯一id  UnitId=Id
        public string AccountName { get; set; }
        
        public PlayerState PlayerState { get; set; }
        public string Name { get; set; }//当前登录角色名字
        public int ConfigId { get; set; }//当前登录角色配置id
    }
}