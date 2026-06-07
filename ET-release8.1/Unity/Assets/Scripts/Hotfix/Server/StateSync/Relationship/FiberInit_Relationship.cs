namespace ET.Server
{
    [Invoke((long)SceneType.Relationship)]
    public class FiberInit_Relationship: AInvokeHandler<FiberInit, ETTask>
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
            
            root.AddComponent<TeamUnitsComponent>();

            await ETTask.CompletedTask;
        }
    }
}

