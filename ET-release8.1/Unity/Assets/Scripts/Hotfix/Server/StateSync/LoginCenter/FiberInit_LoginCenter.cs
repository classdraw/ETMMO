using System.Net;

namespace ET.Server
{
    [Invoke((long)SceneType.LoginCenter)]
    public class FiberInit_LoginCenter: AInvokeHandler<FiberInit, ETTask>
    {
        public override async ETTask Handle(FiberInit fiberInit)
        {
            Scene root = fiberInit.Fiber.Root;
            root.AddComponent<MailBoxComponent, MailBoxType>(MailBoxType.UnOrderedMessage);
            root.AddComponent<TimerComponent>();
            root.AddComponent<CoroutineLockComponent>();
            root.AddComponent<ProcessInnerSender>();
            root.AddComponent<MessageSender>();
            //记录登录数据组件
            root.AddComponent<LoginInfoRecordComponent>();
            await ETTask.CompletedTask;
        }
    }
}