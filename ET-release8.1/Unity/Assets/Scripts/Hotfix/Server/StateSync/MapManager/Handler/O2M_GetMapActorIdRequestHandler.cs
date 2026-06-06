namespace ET.Server
{
    [MessageHandler(SceneType.MapManager)]
    public class O2M_GetMapActorIdRequestHandler: MessageHandler<Scene, O2M_GetMapActorIdRequest, M2O_GetMapActorIdResponse>
    {
        protected override async ETTask Run(Scene scene, O2M_GetMapActorIdRequest request, M2O_GetMapActorIdResponse response)
        {
            (int errno, ActorId actorId) r = await scene.GetComponent<MapManagerComponent>().GetMapActorId(request.MapConfigId, request.Id);
            response.Error = r.errno;
            response.ActorId = r.actorId;
            await ETTask.CompletedTask;
        }
    }
}

