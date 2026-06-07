namespace ET.Server
{
    /// <summary>
    /// 队伍单元，Id = TeamId
    /// </summary>
    [ChildOf(typeof(TeamUnitsComponent))]
    public class TeamUnit : Entity, IAwake, IDestroy
    {
    }
}
