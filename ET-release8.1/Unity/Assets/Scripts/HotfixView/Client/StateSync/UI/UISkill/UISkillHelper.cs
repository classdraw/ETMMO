namespace ET.Client
{
    public static class UISkillHelper
    {
        public static void Open(Scene scene)
        {
            OpenAsync(scene).Coroutine();
        }

        private static async ETTask OpenAsync(Scene scene)
        {
            if (scene == null)
            {
                Log.Error("UISkill 打开失败：CurrentScene 为空");
                return;
            }

            UIComponent uiComponent = scene.GetComponent<UIComponent>();
            if (uiComponent?.Get(UIType.UISkill) != null)
            {
                await UIHelper.Remove(scene, UIType.UISkill);
                return;
            }

            if (uiComponent.Get(UIType.UIBag) != null)
            {
                await UIHelper.Remove(scene, UIType.UIBag);
            }

            UI ui = await UIHelper.Create(scene, UIType.UISkill);
            if (ui == null)
            {
                Log.Error("UISkill 创建失败");
                return;
            }

            if (ui.GetComponent<UISkillComponent>() == null)
            {
                Log.Error("[UISkill] 创建 UI 成功但未挂载 UISkillComponent");
            }
        }
    }
}