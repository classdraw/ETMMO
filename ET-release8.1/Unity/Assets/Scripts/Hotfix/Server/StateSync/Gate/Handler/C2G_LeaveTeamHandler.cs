using System.Collections.Generic;

namespace ET.Server
{
    [MessageSessionHandler(SceneType.Gate)]
    public class C2G_LeaveTeamHandler : MessageSessionHandler<C2G_LeaveTeam, G2C_LeaveTeam>
    {
        protected override async ETTask Run(Session session, C2G_LeaveTeam request, G2C_LeaveTeam response)
        {
            if (session.GetComponent<SessionLockingComponent>() != null)
            {
                response.Error = ErrorCode.ERR_RequestRepeatedly;
                return;
            }

            SessionPlayerComponent sessionPlayerComponent = session.GetComponent<SessionPlayerComponent>();
            if (sessionPlayerComponent == null)
            {
                response.Error = ErrorCode.ERR_SessionPlayerError;
                return;
            }

            Player player = sessionPlayerComponent.Player;
            if (player == null || player.IsDisposed)
            {
                response.Error = ErrorCode.ERR_NonePlayerError;
                return;
            }
            
            using (session.AddComponent<SessionLockingComponent>())
            {
                if (player.IsDisposed)
                {
                    response.Error = ErrorCode.ERR_PlayerSessionError;
                    return;
                }

                if (player.PlayerState != PlayerState.Game)
                {
                    response.Error = ErrorCode.ERR_ErrorEnterGame;
                    return;
                }

                M2G_GetUnitTeamId m2GGetUnitTeamId = (M2G_GetUnitTeamId)await session.Root().GetComponent<MessageLocationSenderComponent>()
                        .Get(LocationType.Unit).Call(player.UnitId, G2M_GetUnitTeamId.Create());
                if (m2GGetUnitTeamId.Error != ErrorCode.ERR_Success)
                {
                    response.Error = m2GGetUnitTeamId.Error;
                    response.Message = m2GGetUnitTeamId.Message;
                    return;
                }

                if (m2GGetUnitTeamId.TeamId <= 0)
                {
                    response.Error = ErrorCode.ERR_TeamNotExist;
                    return;
                }

                long teamId = m2GGetUnitTeamId.TeamId;
                bool dissolve = request.Dissolve > 0;
                (int leaveError, List<long> clearUnitIds) = await RelationshipHelper.LeaveTeam(
                    session.Scene(), player.UnitId, teamId, dissolve);
                if (leaveError != ErrorCode.ERR_Success)
                {
                    response.Error = leaveError;
                    return;
                }

                if (clearUnitIds == null || clearUnitIds.Count == 0)
                {
                    return;
                }

                foreach (long unitId in clearUnitIds)
                {
                    int clearUnitTeamIdError = await RelationshipHelper.ClearUnitTeamId(session.Scene(), unitId);
                    if (clearUnitTeamIdError != ErrorCode.ERR_Success)
                    {
                        response.Error = clearUnitTeamIdError;
                        return;
                    }
                }
            }
        }
    }
}
