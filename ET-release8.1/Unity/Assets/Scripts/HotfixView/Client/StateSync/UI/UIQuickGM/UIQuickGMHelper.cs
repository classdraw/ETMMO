namespace ET.Client
{
    public static class UIQuickGMHelper
    {
        public static void Open(Scene scene)
        {
            OpenAsync(scene).Coroutine();
        }

        private static async ETTask OpenAsync(Scene scene)
        {
            if (scene == null)
            {
                Log.Error("UIQuickGM 打开失败：CurrentScene 为空");
                return;
            }

            UIComponent uiComponent = scene.GetComponent<UIComponent>();
            if (uiComponent?.Get(UIType.UIQuickGM) != null)
            {
                return;
            }

            UI ui = await UIHelper.Create(scene, UIType.UIQuickGM);
            if (ui == null)
            {
                Log.Error("UIQuickGM 创建失败");
                return;
            }

            if (ui.GetComponent<UIQuickGMComponent>() == null)
            {
                Log.Error("[UIQuickGM] 创建 UI 成功但未挂载 UIQuickGMComponent");
            }
        }
    }
}