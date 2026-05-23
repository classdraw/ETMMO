using System.Net;

namespace ET.Server
{
    [Invoke((long)SceneType.Mail)]
    public class FiberInit_Mail: AInvokeHandler<FiberInit, ETTask>
    {
        public override async ETTask Handle(FiberInit fiberInit)
        {
            Scene root = fiberInit.Fiber.Root;
            root.AddComponent<MailBoxComponent, MailBoxType>(MailBoxType.UnOrderedMessage);
            root.AddComponent<TimerComponent>();
            root.AddComponent<CoroutineLockComponent>();
            root.AddComponent<ProcessInnerSender>();
            root.AddComponent<MessageSender>();
            root.AddComponent<LocationProxyComponent>();
            root.AddComponent<MessageLocationSenderComponent>();
            root.AddComponent<DBManagerComponent>();
            
            //mailcenter
            root.AddComponent<MailCenterComponent>();
            //mailunit
            root.AddComponent<MailUnitsComponent>();
            await ETTask.CompletedTask;
        }
    }
}