using System.Collections.Generic;

namespace ET.Server
{
    public static class MapManagerHelper
    {
        /// <summary>
        /// 传送前调用，如果没有这个地图就创建
        /// </summary>
        /// <param name="scene"></param>
        /// <param name="mapConfigId"></param>
        /// <param name="id"></param>
        /// <param name="zone"></param>
        /// <returns></returns>
        public static async ETTask<(int, ActorId)> GetMapActorId(Scene scene,int mapConfigId,long mapFiberId, int? zone = default)
        {
            int zoneId = zone ?? scene.Zone();
            StartSceneConfig sceneConfig = StartSceneConfigCategory.Instance.MapManagerConfigs[zoneId];

            O2M_GetMapActorIdRequest request = O2M_GetMapActorIdRequest.Create();
            request.MapConfigId = mapConfigId;
            request.Id = mapFiberId;
            var resp = await scene.Root().GetComponent<MessageSender>().Call(sceneConfig.ActorId, request) as M2O_GetMapActorIdResponse;
            return (resp.Error, resp.ActorId);
        }
        
        public static void EnterMap(Scene scene, long id, int mapId, ActorId sceneInstanceId)
        {
            StartSceneConfig sceneConfig = StartSceneConfigCategory.Instance.MapManagerConfigs[scene.Zone()];
            O2M_EnterMap request = O2M_EnterMap.Create();
            request.MapConfigId = mapId;
            request.Id = id;
            request.MapActorId = sceneInstanceId;
            scene.Root().GetComponent<MessageSender>().Send(sceneConfig.ActorId, request);
        }
        
        public static async ETTask<(int, ActorId)> CreateMap(Scene scene, int mapId, CreateMapCtx ctx)
        {
            StartSceneConfig sceneConfig = StartSceneConfigCategory.Instance.MapManagerConfigs[scene.Zone()];
            O2M_CreateMapRequest request = O2M_CreateMapRequest.Create();
            request.MapConfigId = mapId;
            request.Ctx = ctx;
            var resp = await scene.Root().GetComponent<MessageSender>().Call(sceneConfig.ActorId, request) as M2O_CreateMapResponse;
            if (resp.Error != ErrorCode.ERR_Success)
            {
                return (resp.Error, default);
            }

            return (ErrorCode.ERR_Success, resp.ActorId);
        }
    }
}

