using System;
using UnityEngine;

namespace ET.Client
{
    [UIEvent(UIType.UIHelp,(int)UISortingOrder.UI,true)]
    [FriendOfAttribute(typeof(ET.Client.UIGlobalComponent))]
    public class UIHelpEvent : AUIEvent
    {
        public override async ETTask<UI> OnCreate(UIComponent uiComponent)
        {
            string assetsName = $"Assets/Bundles/UI/UIHelp/{UIType.UIHelp}.prefab";
            GameObject bundleGameObject = await uiComponent.Scene().GetComponent<ResourcesLoaderComponent>().LoadAssetAsync<GameObject>(assetsName);
            GameObject gameObject = UnityEngine.Object.Instantiate(bundleGameObject, uiComponent.UIGlobalComponent.UICanvas);
            UI ui = uiComponent.AddChild<UI, string, GameObject, int>(UIType.UIHelp, gameObject,
                UIEventComponent.Instance.UISortingOrders[UIType.UIHelp]);

            ui.AddComponent<UIHelpComponent>();
            return ui;
        }

        public override void OnRemove(UIComponent uiComponent)
        {
        }
    }
}