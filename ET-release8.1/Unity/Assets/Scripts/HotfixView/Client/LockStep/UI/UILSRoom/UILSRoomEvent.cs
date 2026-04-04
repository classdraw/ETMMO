using System;
using UnityEngine;

namespace ET.Client
{
    [UIEvent(UIType.UILSRoom)]
    [FriendOfAttribute(typeof(ET.Client.TEngineComponent))]
    public class UILSRoomEvent : AUIEvent
    {
        public override async ETTask<UI> OnCreate(UIComponent uiComponent)
        {
            var uiRoot = uiComponent.Root().GetComponent<TEngineComponent>().UIRootObj;
            string assetsName = $"Assets/Bundles/UI/LockStep/{UIType.UILSRoom}.prefab";
            GameObject bundleGameObject = await uiComponent.Room().GetComponent<ResourcesLoaderComponent>().LoadAssetAsync<GameObject>(assetsName);
            GameObject gameObject = UnityEngine.Object.Instantiate(bundleGameObject, uiRoot.transform);
            UI ui = uiComponent.AddChild<UI, string, GameObject>(UIType.UILSRoom, gameObject);
            ui.AddComponent<UILSRoomComponent>();
            return ui;
        }

        public override void OnRemove(UIComponent uiComponent)
        {
        }
    }
}