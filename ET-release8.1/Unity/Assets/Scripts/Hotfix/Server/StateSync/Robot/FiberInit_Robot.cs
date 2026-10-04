namespace ET.Server
{
    /// <summary>
    /// Process=2 配置里仍有 Robot01 纤程；压测 Run 3 在 Main 控制台执行，本纤程仅占位初始化。
    /// </summary>
    [Invoke((long)SceneType.Robot)]
    public class FiberInit_Robot : AInvokeHandler<FiberInit, ETTask>
    {
        public override async ETTask Handle(FiberInit fiberInit)
        {
            Scene root = fiberInit.Fiber.Root;
            root.AddComponent<MailBoxComponent, MailBoxType>(MailBoxType.UnOrderedMessage);
            root.AddComponent<TimerComponent>();
            root.AddComponent<CoroutineLockComponent>();
            root.AddComponent<ProcessInnerSender>();
            await ETTask.CompletedTask;
        }
    }
}
