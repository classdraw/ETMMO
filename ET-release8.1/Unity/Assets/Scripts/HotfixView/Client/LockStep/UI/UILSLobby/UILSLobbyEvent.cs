using UnityEngine;

namespace ET.Client
{
    [UIEvent(UIType.UILSLobby,(int)UISortingOrder.UI,true)]
    [FriendOfAttribute(typeof(ET.Client.UIGlobalComponent))]
    public class UILSLobbyEvent : AUIEvent
    {
        public override async ETTask<UI> OnCreate(UIComponent uiComponent)
        {
            string assetsName = $"Assets/Bundles/UI/LockStep/{UIType.UILSLobby}.prefab";
            GameObject bundleGameObject = await uiComponent.Scene().GetComponent<ResourcesLoaderComponent>().LoadAssetAsync<GameObject>(assetsName);
            GameObject gameObject = UnityEngine.Object.Instantiate(bundleGameObject, uiComponent.UIGlobalComponent.UICanvas);
            UI ui = uiComponent.AddChild<UI, string, GameObject, int>(UIType.UILSLobby, gameObject,
                UIEventComponent.Instance.UISortingOrders[UIType.UILSLobby]);

            ui.AddComponent<UILSLobbyComponent>();
            return ui;
        }

        public override void OnRemove(UIComponent uiComponent)
        {
        }
    }
}