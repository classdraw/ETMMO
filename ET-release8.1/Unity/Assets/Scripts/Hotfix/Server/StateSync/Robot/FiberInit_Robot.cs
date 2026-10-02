namespace ET.Client
{
    [Invoke((long)SceneType.Robot)]
    public class FiberInit_Robot: AInvokeHandler<FiberInit, ETTask>
    {
        public override async ETTask Handle(FiberInit fiberInit)
        {
            Scene root = fiberInit.Fiber.Root;
            root.AddComponent<MailBoxComponent, MailBoxType>(MailBoxType.UnOrderedMessage);
            root.AddComponent<TimerComponent>();
            root.AddComponent<CoroutineLockComponent>();
            root.AddComponent<ProcessInnerSender>();
            root.AddComponent<PlayerComponent>();
            root.AddComponent<CurrentScenesComponent>();
            root.AddComponent<ObjectWait>();
            root.SceneType = SceneType.StateSync;

            await EventSystem.Instance.PublishAsync(root, new AppStartInitFinish());
            RobotUnitHelper.LogConsole(root.Name, "Console/legacy 纤程，压测 Run 3 不再创建登录纤程");
            await ETTask.CompletedTask;
        }
    }
}
