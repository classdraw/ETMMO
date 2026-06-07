namespace ET.Server
{
    /// <summary>
    /// Relationship 场景上的队伍单元容器（类似 MailUnitsComponent，仅挂载 TeamUnit）
    /// </summary>
    [ComponentOf(typeof(Scene))]
    public class TeamUnitsComponent : Entity, IAwake, IDestroy
    {
    }
}
