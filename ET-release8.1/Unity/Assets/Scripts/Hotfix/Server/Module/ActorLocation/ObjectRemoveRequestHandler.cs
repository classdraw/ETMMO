using System;

namespace ET.Server
{
    [MessageHandler(SceneType.Location)]
    public class ObjectRemoveRequestHandler: MessageHandler<Scene, ObjectRemoveRequest, ObjectRemoveResponse>
    {
        protected override async ETTask Run(Scene root, ObjectRemoveRequest request, ObjectRemoveResponse response)
        {
            await root.GetComponent<LocationManagerComoponent>().Get(request.Type).Remove(request.Key);
        }
    }
}