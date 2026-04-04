using System;
using UnityEngine;

namespace ET.Client
{
    [UIEvent(UIType.UISessionError,(int)UISortingOrder.System,true)]
    [FriendOfAttribute(typeof(ET.Client.UIGlobalComponent))]
    public class UISessionErrorEvent : AUIEvent
    {
        public override async ETTask<UI> OnCreate(UIComponent uiComponent)
        {
            string assetsName = $"Assets/Bundles/UI/Demo/{UIType.UISessionError}.prefab";
            GameObject bundleGameObject = await uiComponent.Scene().GetComponent<ResourcesLoaderComponent>().LoadAssetAsync<GameObject>(assetsName);
            GameObject gameObject = UnityEngine.Object.Instantiate(bundleGameObject, uiComponent.UIGlobalComponent.UICanvas);
            UI ui = uiComponent.AddChild<UI, string, GameObject, int>(UIType.UISessionError, gameObject,
                UIEventComponent.Instance.UISortingOrders[UIType.UISessionError]);
            ui.AddComponent<UISessionErrorComponent>();
            return ui;
        }

        public override void OnRemove(UIComponent uiComponent)
        {
        }
    }
}