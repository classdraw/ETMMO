namespace ET.Server
{
    [MessageHandler(SceneType.Relationship)]
    [FriendOf(typeof(TeamComponent))]
    public class G2Relationship_CreateTeamHandler : MessageHandler<Scene, G2Relationship_CreateTeam, Relationship2G_CreateTeam>
    {
        protected override async ETTask Run(Scene root, G2Relationship_CreateTeam request, Relationship2G_CreateTeam response)
        {
            Log.Console($"[Relationship][CreateTeam] 请求 UnitId={request.UnitId} TeamName={request.TeamName}");

            if (request.UnitId <= 0)
            {
                response.Error = ErrorCode.ERR_NonePlayerError;
                Log.Warning($"[Relationship][CreateTeam] 失败 Error={response.Error} UnitId={request.UnitId}");
                return;
            }

            if (string.IsNullOrWhiteSpace(request.TeamName))
            {
                response.Error = ErrorCode.ERR_TeamNameNull;
                Log.Warning($"[Relationship][CreateTeam] 失败 Error={response.Error} UnitId={request.UnitId}");
                return;
            }

            long teamId = IdGenerater.Instance.GenerateId();
            TeamUnitsComponent teamUnitsComponent = root.GetComponent<TeamUnitsComponent>();

            using (await root.GetComponent<CoroutineLockComponent>().Wait(CoroutineLockType.OperateTeam, teamId))
            {
                TeamUnit teamUnit = teamUnitsComponent.AddChildWithId<TeamUnit>(teamId);
                teamUnit.AddComponent<MailBoxComponent, MailBoxType>(MailBoxType.OrderedMessage);

                TeamComponent teamComponent = teamUnit.AddComponentWithId<TeamComponent>(teamId);
                teamComponent.Name = request.TeamName;
                if (!teamComponent.MemberUnitIds.Contains(request.UnitId))
                {
                    teamComponent.MemberUnitIds.Add(request.UnitId);
                }

                teamComponent.LeaderUnitId = request.UnitId;

                teamComponent.SetOnline(request.UnitId);

                teamComponent.BeginInit();
                await root.GetComponent<DBManagerComponent>().GetZoneDB(root.Zone()).Save(teamComponent);

                await root.GetComponent<LocationProxyComponent>().Add(LocationType.Team, request.UnitId, teamUnit.GetActorId());

                response.TeamId = teamId;
                Log.Console($"[Relationship][CreateTeam] 成功 TeamId={teamId} LeaderUnitId={request.UnitId} MemberCount={teamComponent.MemberUnitIds.Count} Name={request.TeamName}");
            }
        }
    }
}
