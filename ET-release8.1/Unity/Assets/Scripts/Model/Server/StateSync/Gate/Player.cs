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
    public sealed class Player : Entity, IAwake<string, string, string>
    {
        public long UnitId { get; set; }//数据库id 也是唯一id  UnitId=Id
        public string AccountName { get; set; }
        
        public PlayerState PlayerState { get; set; }
        public string Name { get; set; }//当前登录角色名字

        public int Race { get; set; }//种族
        public int Gender { get; set; }//性别
        public int ConfigId { get; set; }//数值配置id
        public string BaseExternalDisplay { get; set; }//当前登录角色外显
    }
}
