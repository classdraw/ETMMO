namespace ET.Server
{
    //登陆玩家实体
    [ChildOf(typeof(PlayerComponent))]
    public sealed class Player : Entity, IAwake<string>
    {
        public string Account { get; set; }
    }
}