using System;

namespace ET.Server
{
    [MessageHandler(SceneType.Location)]
    public class ObjectUnLockRequestHandler: MessageHandler<Scene, ObjectUnLockRequest, ObjectUnLockResponse>
    {
        protected override async ETTask Run(Scene root, ObjectUnLockRequest request, ObjectUnLockResponse response)
        {
            root.GetComponent<LocationManagerComoponent>().Get(request.Type).UnLock(request.Key, request.OldActorId, request.NewActorId);

            await ETTask.CompletedTask;
        }
    }
}