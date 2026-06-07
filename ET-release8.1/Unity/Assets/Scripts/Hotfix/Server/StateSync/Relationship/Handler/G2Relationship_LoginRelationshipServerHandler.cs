namespace ET.Server
{
    [MessageHandler(SceneType.Relationship)]
    [FriendOf(typeof(TeamComponent))]
    public class G2Relationship_LoginRelationshipServerHandler : MessageHandler<Scene, G2Relationship_LoginRelationshipServer, Relationship2G_LoginRelationshipServer>
    {
        protected override async ETTask Run(Scene root, G2Relationship_LoginRelationshipServer request, Relationship2G_LoginRelationshipServer response)
        {
            if (request.TeamId <= 0)
            {
                return;
            }

            TeamUnitsComponent teamUnitsComponent = root.GetComponent<TeamUnitsComponent>();
            using (await root.GetComponent<CoroutineLockComponent>().Wait(CoroutineLockType.OperateTeam, request.TeamId))
            {
                TeamUnit teamUnit = teamUnitsComponent.GetTeamUnit(request.TeamId);
                if (teamUnit != null)
                {
                    TeamComponent existTeamComponent = teamUnit.GetComponent<TeamComponent>();
                    if (existTeamComponent != null && existTeamComponent.OnlineUnitIds.Contains(request.UnitId))
                    {
                        return;
                    }
                }

                TeamComponent teamComponent = teamUnit?.GetComponent<TeamComponent>();
                if (teamComponent == null)
                {
                    teamComponent = await root.GetComponent<DBManagerComponent>().GetZoneDB(root.Zone()).Query<TeamComponent>(request.TeamId);
                }

                if (teamComponent == null)
                {
                    teamUnit?.Dispose();
                    response.ClearUnitTeamId = 1;
                    return;
                }

                teamUnit = teamUnitsComponent.GetOrCreateTeamUnit(request.TeamId);
                if (teamUnit.GetComponent<MailBoxComponent>() == null)
                {
                    teamUnit.AddComponent<MailBoxComponent, MailBoxType>(MailBoxType.OrderedMessage);
                }

                if (teamUnit.GetComponent<TeamComponent>() == null)
                {
                    teamUnit.AddComponent(teamComponent);
                }

                teamComponent.SetOnline(request.UnitId);
                await root.GetComponent<LocationProxyComponent>().Add(LocationType.Team, request.UnitId, teamUnit.GetActorId());
            }
        }
    }
}
