using XEngine.Hud;

namespace ET.Client
{
    [Event(SceneType.StateSync)]
    public class SceneChangeStart_ClearHud: AEvent<Scene, SceneChangeStart>
    {
        protected override async ETTask Run(Scene root, SceneChangeStart args)
        {
            HudFacade.Instance.ClearSceneHud();
            await ETTask.CompletedTask;
        }
    }
}
