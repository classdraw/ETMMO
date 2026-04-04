using UnityEngine;

namespace ET.Client
{
    [UIEvent(UIType.UILobby,(int)UISortingOrder.UI)]
    [FriendOfAttribute(typeof(ET.Client.UIGlobalComponent))]
    public class UILobbyEvent : AUIEvent
    {
        public override async ETTask<UI> OnCreate(UIComponent uiComponent)
        {
            string assetsName = $"Assets/Bundles/UI/Demo/{UIType.UILobby}.prefab";
            GameObject bundleGameObject = await uiComponent.Scene().GetComponent<ResourcesLoaderComponent>().LoadAssetAsync<GameObject>(assetsName);
            GameObject gameObject = UnityEngine.Object.Instantiate(bundleGameObject, uiComponent.UIGlobalComponent.UICanvas);
            UI ui = uiComponent.AddChild<UI, string, GameObject, int>(UIType.UILobby, gameObject,
                UIEventComponent.Instance.UISortingOrders[UIType.UILobby]);

            ui.AddComponent<UILobbyComponent>();
            return ui;
        }

        public override void OnRemove(UIComponent uiComponent)
        {
        }
    }
}