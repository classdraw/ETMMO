namespace ET.Client
{
    [Event(SceneType.All)]
    public class SessionDisposeNotify_CreateUISessionError : AEvent<Scene, SessionDisposeNotify>
    {
        protected override async ETTask Run(Scene root, SessionDisposeNotify args)
        {
            await UIHelper.Create(root, UIType.UISessionError);
        }
    }
}
