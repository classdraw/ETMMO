using System.Collections.Generic;

namespace ET.Server
{
    [MessageHandler(SceneType.Relationship)]
    [FriendOf(typeof(TeamComponent))]
    public class G2Relationship_LeaveTeamHandler : MessageHandler<Scene, G2Relationship_LeaveTeam, Relationship2G_LeaveTeam>
    {
        protected override async ETTask Run(Scene root, G2Relationship_LeaveTeam request, Relationship2G_LeaveTeam response)
        {
            Log.Console($"[Relationship][LeaveTeam] 请求 UnitId={request.UnitId} TeamId={request.TeamId} Dissolve={request.Dissolve}");

            if (request.UnitId <= 0)
            {
                response.Error = ErrorCode.ERR_NonePlayerError;
                Log.Warning($"[Relationship][LeaveTeam] 失败 Error={response.Error} UnitId={request.UnitId} TeamId={request.TeamId}");
                return;
            }

            if (request.TeamId <= 0)
            {
                response.Error = ErrorCode.ERR_TeamNotExist;
                Log.Warning($"[Relationship][LeaveTeam] 失败 Error={response.Error} UnitId={request.UnitId} TeamId={request.TeamId}");
                return;
            }

            TeamUnitsComponent teamUnitsComponent = root.GetComponent<TeamUnitsComponent>();
            using (await root.GetComponent<CoroutineLockComponent>().Wait(CoroutineLockType.OperateTeam, request.TeamId))
            {
                TeamUnit teamUnit = teamUnitsComponent.GetTeamUnit(request.TeamId);
                if (teamUnit == null)
                {
                    response.Error = ErrorCode.ERR_TeamNotExist;
                    Log.Warning($"[Relationship][LeaveTeam] 失败 Error={response.Error} TeamUnit不存在 TeamId={request.TeamId}");
                    return;
                }

                TeamComponent teamComponent = teamUnit.GetComponent<TeamComponent>();
                if (teamComponent == null || !teamComponent.MemberUnitIds.Contains(request.UnitId))
                {
                    response.Error = ErrorCode.ERR_TeamNotInTeam;
                    Log.Warning($"[Relationship][LeaveTeam] 失败 Error={response.Error} UnitId={request.UnitId} TeamId={request.TeamId}");
                    return;
                }

                List<long> clearUnitIds = new();
                if (request.Dissolve > 0)
                {
                    if (teamComponent.LeaderUnitId != request.UnitId)
                    {
                        response.Error = ErrorCode.ERR_TeamNotLeader;
                        Log.Warning($"[Relationship][LeaveTeam] 解散失败 Error={response.Error} UnitId={request.UnitId} LeaderUnitId={teamComponent.LeaderUnitId}");
                        return;
                    }

                    clearUnitIds.AddRange(teamComponent.MemberUnitIds);
                    await DissolveTeam(root, teamUnitsComponent, teamUnit, teamComponent, clearUnitIds);
                    Log.Console($"[Relationship][LeaveTeam] 解散成功 TeamId={request.TeamId} ClearCount={clearUnitIds.Count} ClearUnitIds={string.Join(",", clearUnitIds)}");
                }
                else
                {
                    clearUnitIds.Add(request.UnitId);
                    await LeaveTeam(root, teamUnitsComponent, teamUnit, teamComponent, request.UnitId);
                }

                response.ClearUnitIds.AddRange(clearUnitIds);
            }
        }

        private static async ETTask LeaveTeam(Scene root, TeamUnitsComponent teamUnitsComponent, TeamUnit teamUnit, TeamComponent teamComponent, long unitId)
        {
            teamComponent.MemberUnitIds.Remove(unitId);
            teamComponent.SetOffline(unitId);
            await root.GetComponent<LocationProxyComponent>().Remove(LocationType.Team, unitId);

            if (teamComponent.MemberUnitIds.Count == 0)
            {
                Log.Console($"[Relationship][LeaveTeam] 最后一人离队，自动解散 TeamId={teamComponent.Id} UnitId={unitId}");
                await DissolveTeam(root, teamUnitsComponent, teamUnit, teamComponent, null);
                return;
            }

            if (teamComponent.LeaderUnitId == unitId)
            {
                long oldLeaderUnitId = teamComponent.LeaderUnitId;
                teamComponent.LeaderUnitId = teamComponent.MemberUnitIds[0];
                Log.Console($"[Relationship][LeaveTeam] 队长转移 TeamId={teamComponent.Id} OldLeader={oldLeaderUnitId} NewLeader={teamComponent.LeaderUnitId}");
            }

            teamComponent.BeginInit();
            await root.GetComponent<DBManagerComponent>().GetZoneDB(root.Zone()).Save(teamComponent);
            Log.Console($"[Relationship][LeaveTeam] 离队成功 TeamId={teamComponent.Id} UnitId={unitId} RemainMemberCount={teamComponent.MemberUnitIds.Count} LeaderUnitId={teamComponent.LeaderUnitId}");
        }

        private static async ETTask DissolveTeam(Scene root, TeamUnitsComponent teamUnitsComponent, TeamUnit teamUnit, TeamComponent teamComponent, List<long> clearUnitIds)
        {
            if (clearUnitIds != null)
            {
                foreach (long unitId in clearUnitIds)
                {
                    await root.GetComponent<LocationProxyComponent>().Remove(LocationType.Team, unitId);
                }
            }

            await teamUnitsComponent.RemoveTeamFromDb(teamComponent.Id);
            Log.Console($"[Relationship][LeaveTeam] 队伍已销毁 TeamId={teamComponent.Id} Name={teamComponent.Name}");
            teamUnit.Dispose();
        }
    }
}
