using UnityEngine;

namespace ET.Client
{
    [UIEvent(UIType.UILoading,(int)UISortingOrder.System,true)]
    [FriendOfAttribute(typeof(ET.Client.UIGlobalComponent))]
    public class UILoadingEvent : AUIEvent
    {
        public override async ETTask<UI> OnCreate(UIComponent uiComponent)
        {
            string assetsName = $"Assets/Bundles/UI/Demo/{UIType.UILoading}.prefab";
            GameObject bundleGameObject = await uiComponent.Scene().GetComponent<ResourcesLoaderComponent>().LoadAssetAsync<GameObject>(assetsName);
            GameObject gameObject = UnityEngine.Object.Instantiate(bundleGameObject, uiComponent.UIGlobalComponent.UICanvas);
            UI ui = uiComponent.AddChild<UI, string, GameObject, int>(UIType.UILoading, gameObject,
                UIEventComponent.Instance.UISortingOrders[UIType.UILoading]);

            ui.AddComponent<UILoadingComponent>();
            return ui;
        }

        public override void OnRemove(UIComponent uiComponent)
        {
        }
    }
}