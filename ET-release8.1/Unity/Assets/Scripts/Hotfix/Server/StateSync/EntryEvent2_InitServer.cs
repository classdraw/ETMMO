using System;
using System.Net;

namespace ET.Server
{
    [Event(SceneType.Main)]
    [FriendOfAttribute(typeof(ET.ModeContex))]
    public class EntryEvent2_InitServer : AEvent<Scene, EntryEvent2>
    {
        protected override async ETTask Run(Scene root, EntryEvent2 args)
        {
            switch (Options.Instance.AppType)
            {
                case AppType.Server:
                    {
                        int process = root.Fiber.Process;
                        StartProcessConfig startProcessConfig = StartProcessConfigCategory.Instance.Get(process);
                        if (startProcessConfig.Port != 0)
                        {
                            await FiberManager.Instance.Create(SchedulerType.ThreadPool, ConstFiberId.NetInner, 0, SceneType.NetInner, "NetInner");
                        }

                        // 根据配置创建纤程
                        var processScenes = StartSceneConfigCategory.Instance.GetByProcess(process);
                        foreach (StartSceneConfig startConfig in processScenes)
                        {
                            await FiberManager.Instance.Create(SchedulerType.ThreadPool, startConfig.Id, startConfig.Zone, startConfig.Type, startConfig.Name);
                        }

                        break;
                    }
                case AppType.Watcher:
                    {
                        root.AddComponent<WatcherComponent>();
                        break;
                    }
                case AppType.GameTool:
                    {
                        break;
                    }
            }

            if (Options.Instance.Console == 1)
            {
                // 机器人控制台需跨纤程/跨进程 Call MapManager 等
                root.AddComponent<MessageSender>();
                ConsoleComponent consoleComponent = root.AddComponent<ConsoleComponent>();
                root.AddComponent<RobotCaseComponent>();

                if (root.Fiber.Process == 2)
                {
                    ModeContex modeContex = consoleComponent.AddComponent<ModeContex>();
                    modeContex.Mode = ConsoleMode.Robot;
                    Log.Console("[Robot] Process=2 已就绪。输入: Run 3 10 10001（地图服刷 Robot9101，无需账号）");
                }
            }
        }
    }
}