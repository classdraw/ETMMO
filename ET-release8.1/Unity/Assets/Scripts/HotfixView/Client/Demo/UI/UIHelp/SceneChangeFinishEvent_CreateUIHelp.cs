namespace ET.Client
{
    [Event(SceneType.Current)]
    public class SceneChangeFinishEvent_CreateUIHelp : AEvent<Scene, SceneChangeFinish>
    {
        private const string LoginSceneName = "Login";

        protected override async ETTask Run(Scene scene, SceneChangeFinish args)
        {
            if (scene.Name == LoginSceneName)
            {
                await UIHelper.Create(scene.Root(), UIType.UILogin);
            }
            else
            {
                await UIHelper.Create(scene, UIType.UIHelp);
            }
        }
    }
}
