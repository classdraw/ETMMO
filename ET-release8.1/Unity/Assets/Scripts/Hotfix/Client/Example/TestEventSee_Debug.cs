namespace ET.Client
{
    [Event(SceneType.Demo)]
    public class TestEventSee_Debug: AEvent<Scene, TestEventSee>
    {
        protected override async ETTask Run(Scene scene, TestEventSee a)
        {
            Log.Info("0000000000000000000");
            await scene.GetComponent<TimerComponent>().WaitAsync(2000);
            Log.Info("1111111111111111111");

            await Test1(scene);
            int k=await Test2(scene);
            Log.Info(k.ToString());
            await ETTask.CompletedTask;
        }

        private async ETTask Test1(Scene scene)
        {
            Log.Info("MMMMMMMMM");
            await scene.GetComponent<TimerComponent>().WaitAsync(1000);
            Log.Info("NNNNNNNNN");
        }

        private async ETTask<int> Test2(Scene scene)
        {
            Log.Info("ETTask Test2");
            int m = 100;
            for (int i=0;i<3;i++) {
                await scene.GetComponent<TimerComponent>().WaitAsync(1000);
                m++;
            }

            return m;

        }
    }
    
}

