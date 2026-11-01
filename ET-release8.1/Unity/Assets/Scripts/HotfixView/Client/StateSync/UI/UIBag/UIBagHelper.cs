namespace ET.Client
{
    public static class UIBagHelper
    {
        public static void Open(Scene scene)
        {
            OpenAsync(scene).Coroutine();
        }

        private static async ETTask OpenAsync(Scene scene)
        {
            if (scene == null)
            {
                Log.Error("UIBag 打开失败：CurrentScene 为空");
                return;
            }

            UI ui = await UIHelper.Create(scene, UIType.UIBag);
            if (ui == null)
            {
                Log.Error("UIBag 创建失败");
                return;
            }

            UIBagComponent bag = ui.GetComponent<UIBagComponent>();
            if (bag == null)
            {
                Log.Error("[UIBag] UIBagHelper 创建 UI 成功但未挂载 UIBagComponent");
                return;
            }

            await bag.ReloadFromServerAsync();
        }
    }
}
