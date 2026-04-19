namespace ET.Client
{
    [Event(SceneType.StateSync)]
    public class SceneChangeFinish_RemoveLoading: AEvent<Scene, SceneChangeFinish>
    {
        protected override async ETTask Run(Scene root, SceneChangeFinish args)
        {
            await root.Root().GetComponent<TimerComponent>().WaitAsync(2000);
            await UIHelper.Remove(root, UIType.UILoading);
        }
    }
}

