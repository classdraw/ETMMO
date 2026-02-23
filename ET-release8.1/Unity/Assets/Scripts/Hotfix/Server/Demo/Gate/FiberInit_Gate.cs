using System.Net;

namespace ET.Server
{
    [Invoke((long)SceneType.Gate)]
    public class FiberInit_Gate: AInvokeHandler<FiberInit, ETTask>
    {
        public override async ETTask Handle(FiberInit fiberInit)
        {
            Scene root = fiberInit.Fiber.Root;
            root.AddComponent<MailBoxComponent, MailBoxType>(MailBoxType.UnOrderedMessage);
            root.AddComponent<TimerComponent>();//计时器组件
            root.AddComponent<CoroutineLockComponent>();//携程锁
            root.AddComponent<ProcessInnerSender>();//进程内网络通信
            root.AddComponent<MessageSender>();//
            root.AddComponent<PlayerComponent>();//登陆玩家数据对象列表
            root.AddComponent<GateSessionKeyComponent>();//网关的key组件
            root.AddComponent<LocationProxyComponent>();
            root.AddComponent<MessageLocationSenderComponent>();

            StartSceneConfig startSceneConfig = StartSceneConfigCategory.Instance.Get((int)root.Id);
            root.AddComponent<NetComponent, IPEndPoint, NetworkProtocol>(startSceneConfig.InnerIPPort, NetworkProtocol.UDP);
            await ETTask.CompletedTask;
        }
    }
}