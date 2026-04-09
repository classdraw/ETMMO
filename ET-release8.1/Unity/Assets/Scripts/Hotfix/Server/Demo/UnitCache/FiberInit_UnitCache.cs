using System.Net;

namespace ET.Server
{
    [Invoke((long)SceneType.UnitCache)]
    public class FiberInit_UnitCache: AInvokeHandler<FiberInit, ETTask>
    {
        public override async ETTask Handle(FiberInit fiberInit)
        {
            Scene root = fiberInit.Fiber.Root;
            root.AddComponent<MailBoxComponent, MailBoxType>(MailBoxType.UnOrderedMessage);
            root.AddComponent<TimerComponent>();
            root.AddComponent<CoroutineLockComponent>();
            root.AddComponent<ProcessInnerSender>();
            root.AddComponent<MessageSender>();
            //本地内部定位发送
            root.AddComponent<LocationProxyComponent>();
            root.AddComponent<MessageLocationSenderComponent>();
            
            //数据库
            root.AddComponent<DBManagerComponent>();
            root.AddComponent<UnitCacheComponent>();//角色缓存服务器
            await ETTask.CompletedTask;
        }
    }
}

