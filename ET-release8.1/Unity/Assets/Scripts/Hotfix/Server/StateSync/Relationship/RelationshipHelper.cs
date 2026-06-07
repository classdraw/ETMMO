using System.Collections.Generic;

namespace ET.Server
{
    public static class RelationshipHelper
    {
        /// <summary>
        /// 登录 Relationship 服，按 Unit 上的 TeamId 加载队伍数据
        /// </summary>
        public static async ETTask<int> LoginRelationshipServer(Scene scene, Unit unit)
        {
            if (unit == null || unit.IsDisposed)
            {
                return ErrorCode.ERR_NonePlayerError;
            }

            if (unit.TeamId <= 0)
            {
                return ErrorCode.ERR_Success;
            }

            if (!StartSceneConfigCategory.Instance.RelationshipConfigs.TryGetValue(scene.Zone(), out StartSceneConfig startSceneConfig))
            {
                Log.Warning($"[Relationship] 未配置 Relationship 场景 Zone={scene.Zone()}");
                return ErrorCode.ERR_Success;
            }

            G2Relationship_LoginRelationshipServer request = G2Relationship_LoginRelationshipServer.Create();
            request.UnitId = unit.Id;
            request.TeamId = unit.TeamId;
            Relationship2G_LoginRelationshipServer response = (Relationship2G_LoginRelationshipServer)await scene.Root().GetComponent<MessageSender>()
                    .Call(startSceneConfig.ActorId, request);
            if (response.Error != ErrorCode.ERR_Success)
            {
                return response.Error;
            }

            if (response.ClearUnitTeamId > 0)
            {
                unit.TeamId = 0;
                unit.GetComponent<UnitDBSaveComponent>()?.MarkUnitDirty();
            }

            return ErrorCode.ERR_Success;
        }

        /// <summary>
        /// 在 Relationship 服创建队伍
        /// </summary>
        public static async ETTask<(int error, long teamId)> CreateTeam(Scene scene, long unitId, string teamName)
        {
            if (unitId <= 0)
            {
                return (ErrorCode.ERR_NonePlayerError, 0);
            }

            if (string.IsNullOrWhiteSpace(teamName))
            {
                return (ErrorCode.ERR_TeamNameNull, 0);
            }

            if (!StartSceneConfigCategory.Instance.RelationshipConfigs.TryGetValue(scene.Zone(), out StartSceneConfig startSceneConfig))
            {
                Log.Warning($"[Relationship] 未配置 Relationship 场景 Zone={scene.Zone()}");
                return (ErrorCode.ERR_Success, 0);
            }

            G2Relationship_CreateTeam request = G2Relationship_CreateTeam.Create();
            request.UnitId = unitId;
            request.TeamName = teamName;
            Relationship2G_CreateTeam response = (Relationship2G_CreateTeam)await scene.Root().GetComponent<MessageSender>()
                    .Call(startSceneConfig.ActorId, request);
            return (response.Error, response.TeamId);
        }

        /// <summary>
        /// 从 Map 服读取 Unit 上的 TeamId
        /// </summary>
        public static async ETTask<(int error, long teamId)> GetUnitTeamId(Scene scene, long unitId)
        {
            if (unitId <= 0)
            {
                return (ErrorCode.ERR_NonePlayerError, 0);
            }

            M2G_GetUnitTeamId response = (M2G_GetUnitTeamId)await scene.Root().GetComponent<MessageLocationSenderComponent>()
                    .Get(LocationType.Unit).Call(unitId, G2M_GetUnitTeamId.Create());
            return (response.Error, response.TeamId);
        }

        /// <summary>
        /// 同步 Unit 上的 TeamId 到 Map 服
        /// </summary>
        public static async ETTask<int> SetUnitTeamId(Scene scene, long unitId, long teamId)
        {
            if (unitId <= 0)
            {
                return ErrorCode.ERR_NonePlayerError;
            }

            G2M_SetUnitTeamId request = G2M_SetUnitTeamId.Create();
            request.TeamId = teamId;
            M2G_SetUnitTeamId response = (M2G_SetUnitTeamId)await scene.Root().GetComponent<MessageLocationSenderComponent>()
                    .Get(LocationType.Unit).Call(unitId, request);
            return response.Error;
        }

        /// <summary>
        /// 清除 Unit 上的 TeamId
        /// </summary>
        public static async ETTask<int> ClearUnitTeamId(Scene scene, long unitId)
        {
            return await SetUnitTeamId(scene, unitId, 0);
        }

        /// <summary>
        /// 离队或解散队伍
        /// </summary>
        public static async ETTask<(int error, List<long> clearUnitIds)> LeaveTeam(Scene scene, long unitId, long teamId, bool dissolve)
        {
            if (unitId <= 0)
            {
                return (ErrorCode.ERR_NonePlayerError, null);
            }

            if (teamId <= 0)
            {
                return (ErrorCode.ERR_TeamNotExist, null);
            }

            if (!StartSceneConfigCategory.Instance.RelationshipConfigs.TryGetValue(scene.Zone(), out StartSceneConfig startSceneConfig))
            {
                Log.Warning($"[Relationship] 未配置 Relationship 场景 Zone={scene.Zone()}");
                return (ErrorCode.ERR_Success, null);
            }

            G2Relationship_LeaveTeam request = G2Relationship_LeaveTeam.Create();
            request.UnitId = unitId;
            request.TeamId = teamId;
            request.Dissolve = dissolve ? 1 : 0;
            Relationship2G_LeaveTeam response = (Relationship2G_LeaveTeam)await scene.Root().GetComponent<MessageSender>()
                    .Call(startSceneConfig.ActorId, request);
            return (response.Error, response.ClearUnitIds);
        }

        /// <summary>
        /// 退出 Relationship 服，保存队伍数据并销毁 TeamUnit
        /// </summary>
        public static async ETTask<int> ExitRelationshipServer(Scene scene, long unitId, long teamId)
        {
            if (unitId <= 0)
            {
                return ErrorCode.ERR_NonePlayerError;
            }

            if (teamId <= 0)
            {
                return ErrorCode.ERR_Success;
            }

            if (!StartSceneConfigCategory.Instance.RelationshipConfigs.TryGetValue(scene.Zone(), out StartSceneConfig startSceneConfig))
            {
                Log.Warning($"[Relationship] 未配置 Relationship 场景 Zone={scene.Zone()}");
                return ErrorCode.ERR_Success;
            }

            G2Relationship_ExitRelationshipServer request = G2Relationship_ExitRelationshipServer.Create();
            request.UnitId = unitId;
            request.TeamId = teamId;
            Relationship2G_ExitRelationshipServer response = (Relationship2G_ExitRelationshipServer)await scene.Root().GetComponent<MessageSender>()
                    .Call(startSceneConfig.ActorId, request);
            return response.Error;
        }
    }
}
