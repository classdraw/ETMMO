using UnityEngine;

namespace ET.Client
{
    [UIEvent(UIType.UILSLobby)]
    [FriendOfAttribute(typeof(ET.Client.TEngineComponent))]
    public class UILSLobbyEvent : AUIEvent
    {
        public override async ETTask<UI> OnCreate(UIComponent uiComponent)
        {
            var uiRoot = uiComponent.Root().GetComponent<TEngineComponent>().UIRootObj;
            string assetsName = $"Assets/Bundles/UI/LockStep/{UIType.UILSLobby}.prefab";
            GameObject bundleGameObject = await uiComponent.Scene().GetComponent<ResourcesLoaderComponent>().LoadAssetAsync<GameObject>(assetsName);
            GameObject gameObject = UnityEngine.Object.Instantiate(bundleGameObject, uiRoot.transform);
            UI ui = uiComponent.AddChild<UI, string, GameObject>(UIType.UILSLobby, gameObject);

            ui.AddComponent<UILSLobbyComponent>();
            return ui;
        }

        public override void OnRemove(UIComponent uiComponent)
        {
        }
    }
}