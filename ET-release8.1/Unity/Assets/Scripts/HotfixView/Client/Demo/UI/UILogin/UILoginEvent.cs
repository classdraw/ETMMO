using System;
using UnityEngine;

namespace ET.Client
{
    [UIEvent(UIType.UILogin,(int)UISortingOrder.UI,true)]
    [FriendOfAttribute(typeof(ET.Client.UIGlobalComponent))]
    public class UILoginEvent : AUIEvent
    {
        public override async ETTask<UI> OnCreate(UIComponent uiComponent)
        {
            string assetsName = $"Assets/Bundles/UI/Demo/{UIType.UILogin}.prefab";
            GameObject bundleGameObject = await uiComponent.Scene().GetComponent<ResourcesLoaderComponent>().LoadAssetAsync<GameObject>(assetsName);
            GameObject gameObject = UnityEngine.Object.Instantiate(bundleGameObject, uiComponent.UIGlobalComponent.UICanvas);
            UI ui = uiComponent.AddChild<UI, string, GameObject, int>(UIType.UILogin, gameObject,
                UIEventComponent.Instance.UISortingOrders[UIType.UILogin]);
            ui.AddComponent<UILoginComponent>();
            return ui;
        }

        public override void OnRemove(UIComponent uiComponent)
        {
        }
    }
}