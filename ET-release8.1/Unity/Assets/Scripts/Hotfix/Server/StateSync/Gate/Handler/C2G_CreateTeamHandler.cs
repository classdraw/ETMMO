namespace ET.Server
{
    [MessageSessionHandler(SceneType.Gate)]
    public class C2G_CreateTeamHandler : MessageSessionHandler<C2G_CreateTeam, G2C_CreateTeam>
    {
        protected override async ETTask Run(Session session, C2G_CreateTeam request, G2C_CreateTeam response)
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

                if (m2GGetUnitTeamId.TeamId > 0)
                {
                    response.Error = ErrorCode.ERR_TeamAlreadyExist;
                    return;
                }

                (int createError, long teamId) = await RelationshipHelper.CreateTeam(session.Scene(), player.UnitId, request.TeamName);
                if (createError != ErrorCode.ERR_Success)
                {
                    response.Error = createError;
                    return;
                }

                int setTeamIdError = await RelationshipHelper.SetUnitTeamId(session.Scene(), player.UnitId, teamId);
                if (setTeamIdError != ErrorCode.ERR_Success)
                {
                    response.Error = setTeamIdError;
                    return;
                }
            }
        }
    }
}
