namespace ET.Server
{
    [MessageHandler(SceneType.Relationship)]
    [FriendOf(typeof(TeamComponent))]
    public class G2Relationship_ExitRelationshipServerHandler : MessageHandler<Scene, G2Relationship_ExitRelationshipServer, Relationship2G_ExitRelationshipServer>
    {
        protected override async ETTask Run(Scene root, G2Relationship_ExitRelationshipServer request, Relationship2G_ExitRelationshipServer response)
        {
            if (request.TeamId <= 0)
            {
                return;
            }

            TeamUnitsComponent teamUnitsComponent = root.GetComponent<TeamUnitsComponent>();
            using (await root.GetComponent<CoroutineLockComponent>().Wait(CoroutineLockType.OperateTeam,request.TeamId))
            {
                TeamUnit teamUnit = teamUnitsComponent.GetTeamUnit(request.TeamId);
                if (teamUnit == null)
                {
                    return;
                }
                TeamComponent teamComponent = teamUnit.GetComponent<TeamComponent>();
                teamComponent?.SetOffline(request.UnitId);

                if (teamComponent != null && teamComponent.Id > 0)
                {
                    teamComponent.BeginInit();
                    await root.GetComponent<DBManagerComponent>().GetZoneDB(root.Zone()).Save(teamComponent);
                }
                
                TeamUnitExit(root, teamUnit, request.UnitId).Coroutine();
            }

            await ETTask.CompletedTask;
        }

        private async ETTask TeamUnitExit(Scene root, TeamUnit teamUnit, long unitId)
        {
            await teamUnit.Fiber().WaitFrameFinish();
            await root.GetComponent<LocationProxyComponent>().Remove(LocationType.Team, unitId);

            TeamComponent teamComponent = teamUnit.GetComponent<TeamComponent>();
            if (teamComponent != null && teamComponent.OnlineUnitIds.Count > 0)
            {
                return;
            }

            teamUnit?.Dispose();
        }
    }
}
