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

    }
}

