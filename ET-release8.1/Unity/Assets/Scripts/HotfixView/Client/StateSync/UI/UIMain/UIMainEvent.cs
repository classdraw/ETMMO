using System;
using UnityEngine;

namespace ET.Client
{
    [UIEvent(UIType.UIMain, (int)UISortingOrder.Bottom, false)]
    [FriendOfAttribute(typeof(ET.Client.UIGlobalComponent))]
    public class UIMainEvent : AUIEvent
    {
        public override async ETTask<UI> OnCreate(UIComponent uiComponent)
        {
            string assetsName = $"Assets/Bundles/UI/UIMain/{UIType.UIMain}.prefab";
            GameObject bundleGameObject = await uiComponent.Scene().GetComponent<ResourcesLoaderComponent>().LoadAssetAsync<GameObject>(assetsName);
            GameObject gameObject = UnityEngine.Object.Instantiate(bundleGameObject, uiComponent.UIGlobalComponent.UICanvas);
            UI ui = uiComponent.AddChild<UI, string, GameObject, int>(UIType.UIMain, gameObject,
                UIEventComponent.Instance.UISortingOrders[UIType.UIMain]);
            ui.AddComponent<UIMainComponent>();
            return ui;
        }

        public override void OnRemove(UIComponent uiComponent)
        {
        }
    }
}
