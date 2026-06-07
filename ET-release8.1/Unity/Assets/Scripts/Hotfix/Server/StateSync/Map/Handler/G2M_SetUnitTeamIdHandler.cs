namespace ET.Server
{
    [MessageLocationHandler(SceneType.Map)]
    public class G2M_SetUnitTeamIdHandler : MessageLocationHandler<Unit, G2M_SetUnitTeamId, M2G_SetUnitTeamId>
    {
        protected override async ETTask Run(Unit unit, G2M_SetUnitTeamId request, M2G_SetUnitTeamId response)
        {
            await ETTask.CompletedTask;

            unit.TeamId = request.TeamId;
            unit.GetComponent<UnitDBSaveComponent>()?.SaveChangeNoWait();
        }
    }
}
