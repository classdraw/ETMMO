using System;
using UnityEngine;

namespace ET.Client
{
    [UIEvent(UIType.UILSLogin,(int)UISortingOrder.UI,true)]
    [FriendOfAttribute(typeof(ET.Client.UIGlobalComponent))]
    public class UILSLoginEvent : AUIEvent
    {
        public override async ETTask<UI> OnCreate(UIComponent uiComponent)
        {
            string assetsName = $"Assets/Bundles/UI/LockStep/{UIType.UILSLogin}.prefab";
            GameObject bundleGameObject = await uiComponent.Scene().GetComponent<ResourcesLoaderComponent>().LoadAssetAsync<GameObject>(assetsName);
            GameObject gameObject = UnityEngine.Object.Instantiate(bundleGameObject, uiComponent.UIGlobalComponent.UICanvas);
            UI ui = uiComponent.AddChild<UI, string, GameObject, int>(UIType.UILSLogin, gameObject,
                UIEventComponent.Instance.UISortingOrders[UIType.UILSLogin]);
            ui.AddComponent<UILSLoginComponent>();
            return ui;
        }

        public override void OnRemove(UIComponent uiComponent)
        {
        }
    }
}