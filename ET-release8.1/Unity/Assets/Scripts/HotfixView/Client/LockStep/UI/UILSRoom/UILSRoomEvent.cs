using System;
using UnityEngine;

namespace ET.Client
{
    [UIEvent(UIType.UILSRoom,(int)UISortingOrder.UI,true)]
    [FriendOfAttribute(typeof(ET.Client.UIGlobalComponent))]
    public class UILSRoomEvent : AUIEvent
    {
        public override async ETTask<UI> OnCreate(UIComponent uiComponent)
        {
            string assetsName = $"Assets/Bundles/UI/LockStep/{UIType.UILSRoom}.prefab";
            GameObject bundleGameObject = await uiComponent.Room().GetComponent<ResourcesLoaderComponent>().LoadAssetAsync<GameObject>(assetsName);
            GameObject gameObject = UnityEngine.Object.Instantiate(bundleGameObject, uiComponent.UIGlobalComponent.UICanvas);
            UI ui = uiComponent.AddChild<UI, string, GameObject, int>(UIType.UILSRoom, gameObject,
                UIEventComponent.Instance.UISortingOrders[UIType.UILSRoom]);
            ui.AddComponent<UILSRoomComponent>();
            return ui;
        }

        public override void OnRemove(UIComponent uiComponent)
        {
        }
    }
}