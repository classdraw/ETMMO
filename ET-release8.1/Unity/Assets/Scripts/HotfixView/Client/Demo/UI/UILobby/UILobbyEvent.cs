using UnityEngine;

namespace ET.Client
{
    [UIEvent(UIType.UILobby)]
    [FriendOfAttribute(typeof(ET.Client.TEngineComponent))]
    public class UILobbyEvent : AUIEvent
    {
        public override async ETTask<UI> OnCreate(UIComponent uiComponent)
        {
            var uiRoot = uiComponent.Root().GetComponent<TEngineComponent>().UIRootObj;
            string assetsName = $"Assets/Bundles/UI/Demo/{UIType.UILobby}.prefab";
            GameObject bundleGameObject = await uiComponent.Scene().GetComponent<ResourcesLoaderComponent>().LoadAssetAsync<GameObject>(assetsName);
            GameObject gameObject = UnityEngine.Object.Instantiate(bundleGameObject, uiRoot.transform);
            UI ui = uiComponent.AddChild<UI, string, GameObject>(UIType.UILobby, gameObject);

            ui.AddComponent<UILobbyComponent>();
            return ui;
        }

        public override void OnRemove(UIComponent uiComponent)
        {
        }
    }
}