namespace ET.Client
{
    [Event(SceneType.Demo)]
    public class SceneChangeStart_CreateLoading: AEvent<Scene, SceneChangeStart>
    {
        protected override async ETTask Run(Scene root, SceneChangeStart args)
        {
            await UIHelper.Create(root, UIType.UILoading);
        }
    }
}

