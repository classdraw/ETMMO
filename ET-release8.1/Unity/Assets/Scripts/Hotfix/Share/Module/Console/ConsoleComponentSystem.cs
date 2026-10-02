using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ET
{
    [EntitySystemOf(typeof(ConsoleComponent))]
    [FriendOf(typeof(ConsoleComponent))]
    [FriendOf(typeof(ModeContex))]
    public static partial class ConsoleComponentSystem
    {
        [EntitySystem]
        private static void Awake(this ConsoleComponent self)
        {
            self.Start().Coroutine();
        }

        
        private static async ETTask Start(this ConsoleComponent self)
        {
            self.CancellationTokenSource = new CancellationTokenSource();

            while (true)
            {
                try
                {
                    ModeContex modeContex = self.GetComponent<ModeContex>();
                    string line = await Task.Factory.StartNew(() =>
                    {
                        Console.Write($"{modeContex?.Mode ?? ""}> ");
                        Console.Out.Flush();
                        return Console.In.ReadLine();
                    }, self.CancellationTokenSource.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
                    
                    line = line.Trim();
                    // ReadLine 阻塞期间可能已添加 ModeContex（如 Process=2 初始化），必须重新取
                    modeContex = self.GetComponent<ModeContex>();

                    switch (line)
                    {
                        case "":
                            break;
                        case "exit":
                            self.RemoveComponent<ModeContex>();
                            break;
                        default:
                        {
                            string[] lines = line.Split(" ");
                            string mode = modeContex == null? lines[0] : modeContex.Mode;

                            if (modeContex == null && line.StartsWith("Run 3 "))
                            {
                                if (ConsoleDispatcher.Instance.TryGet(ConsoleMode.Robot, out IConsoleHandler robotHandler))
                                {
                                    modeContex = self.EnsureModeContex(ConsoleMode.Robot);
                                    mode = ConsoleMode.Robot;
                                    await robotHandler.Run(self.Fiber(), modeContex, line);
                                    break;
                                }
                            }

                            if (!ConsoleDispatcher.Instance.TryGet(mode, out IConsoleHandler iConsoleHandler))
                            {
                                if (modeContex == null && mode == "Run")
                                {
                                    Log.Console("机器人窗口输入: Run 3 <数量> <地图Id>  示例: Run 3 10 10001");
                                }
                                else
                                {
                                    Log.Console($"未知控制台命令: {mode}，可用模式例如: Robot");
                                }
                                break;
                            }
                            modeContex = self.EnsureModeContex(mode);
                            await iConsoleHandler.Run(self.Fiber(), modeContex, line);
                            break;
                        }
                    }


                }
                catch (Exception e)
                {
                    Log.Console(e.ToString());
                }
            }
        }

        private static ModeContex EnsureModeContex(this ConsoleComponent self, string mode)
        {
            ModeContex modeContex = self.GetComponent<ModeContex>();
            if (modeContex == null)
            {
                modeContex = self.AddComponent<ModeContex>();
            }

            modeContex.Mode = mode;
            return modeContex;
        }
    }
}