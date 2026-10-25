using System;
using UnityEngine;

namespace ET.Client
{
    [UIEvent(UIType.UIBag, (int)UISortingOrder.Popup, false)]
    [FriendOfAttribute(typeof(ET.Client.UIGlobalComponent))]
    public class UIBagEvent : AUIEvent
    {
        public override async ETTask<UI> OnCreate(UIComponent uiComponent)
        {
            string assetsName = $"Assets/Bundles/UI/UIBag/{UIType.UIBag}.prefab";
            GameObject bundleGameObject = await uiComponent.Scene().GetComponent<ResourcesLoaderComponent>().LoadAssetAsync<GameObject>(assetsName);
            GameObject gameObject = UnityEngine.Object.Instantiate(bundleGameObject, uiComponent.UIGlobalComponent.UICanvas);
            UI ui = uiComponent.AddChild<UI, string, GameObject, int>(UIType.UIBag, gameObject,
                UIEventComponent.Instance.UISortingOrders[UIType.UIBag]);
            ui.AddComponent<UIBagComponent>();
            return ui;
        }

        public override void OnRemove(UIComponent uiComponent)
        {
        }
    }
}
