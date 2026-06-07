namespace ET.Server
{
    [MessageLocationHandler(SceneType.Map)]
    public class G2M_GetUnitTeamIdHandler : MessageLocationHandler<Unit, G2M_GetUnitTeamId, M2G_GetUnitTeamId>
    {
        protected override async ETTask Run(Unit unit, G2M_GetUnitTeamId request, M2G_GetUnitTeamId response)
        {
            await ETTask.CompletedTask;
            response.TeamId = unit.TeamId;
        }
    }
}
