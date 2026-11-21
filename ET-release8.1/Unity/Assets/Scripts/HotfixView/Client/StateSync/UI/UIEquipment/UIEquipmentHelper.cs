namespace ET.Client
{
    /// <summary>装备界面开关。不参与背包/技能互斥，可与背包、技能及其它弹窗同时打开。</summary>
    public static class UIEquipmentHelper
    {
        public static void Open(Scene scene)
        {
            OpenAsync(scene).Coroutine();
        }

        private static async ETTask OpenAsync(Scene scene)
        {
            if (scene == null)
            {
                Log.Error("UIEquipment 打开失败：CurrentScene 为空");
                return;
            }

            UIComponent uiComponent = scene.GetComponent<UIComponent>();
            UI existing = uiComponent?.Get(UIType.UIEquipment);
            if (existing != null)
            {
                UIEquipmentComponent equipment = existing.GetComponent<UIEquipmentComponent>();
                if (equipment != null)
                {
                    await equipment.RequestCloseAsync();
                }
                else
                {
                    await UIHelper.Remove(scene, UIType.UIEquipment);
                }

                return;
            }

            UI ui = await UIHelper.Create(scene, UIType.UIEquipment);
            if (ui == null)
            {
                Log.Error("UIEquipment 创建失败");
                return;
            }

            if (ui.GetComponent<UIEquipmentComponent>() == null)
            {
                Log.Error("[UIEquipment] 创建 UI 成功但未挂载 UIEquipmentComponent");
            }
        }
    }
}