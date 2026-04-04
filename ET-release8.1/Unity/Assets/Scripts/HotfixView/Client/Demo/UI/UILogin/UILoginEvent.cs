using System;
using UnityEngine;

namespace ET.Client
{
    [UIEvent(UIType.UILogin)]
    [FriendOfAttribute(typeof(ET.Client.TEngineComponent))]
    public class UILoginEvent : AUIEvent
    {
        public override async ETTask<UI> OnCreate(UIComponent uiComponent)
        {
            var uiRoot = uiComponent.Root().GetComponent<TEngineComponent>().UIRootObj;
            string assetsName = $"Assets/Bundles/UI/Demo/{UIType.UILogin}.prefab";
            GameObject bundleGameObject = await uiComponent.Scene().GetComponent<ResourcesLoaderComponent>().LoadAssetAsync<GameObject>(assetsName);
            GameObject gameObject = UnityEngine.Object.Instantiate(bundleGameObject, uiRoot.transform);
            UI ui = uiComponent.AddChild<UI, string, GameObject>(UIType.UILogin, gameObject);
            ui.AddComponent<UILoginComponent>();
            return ui;
        }

        public override void OnRemove(UIComponent uiComponent)
        {
        }
    }
}