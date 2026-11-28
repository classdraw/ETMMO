using UnityEngine;

namespace ET.Client
{
    [UIEvent(UIType.UIQuickGM, (int)UISortingOrder.Popup, false)]
    [FriendOf(typeof(UIGlobalComponent))]
    public class UIQuickGMEvent : AUIEvent
    {
        public override async ETTask<UI> OnCreate(UIComponent uiComponent)
        {
            string assetsName = $"Assets/Bundles/UI/UIQuickGM/{UIType.UIQuickGM}.prefab";
            GameObject bundleGameObject = await uiComponent.Scene().GetComponent<ResourcesLoaderComponent>().LoadAssetAsync<GameObject>(assetsName);
            GameObject gameObject = UnityEngine.Object.Instantiate(bundleGameObject, uiComponent.UIGlobalComponent.UICanvas);
            UI ui = uiComponent.AddChild<UI, string, GameObject, int>(UIType.UIQuickGM, gameObject,
                UIEventComponent.Instance.UISortingOrders[UIType.UIQuickGM]);

            ui.AddComponent<UIQuickGMComponent>();
            return ui;
        }

        public override void OnRemove(UIComponent uiComponent)
        {
        }
    }
}