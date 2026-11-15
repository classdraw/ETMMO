using UnityEngine;

namespace ET.Client
{
    [UIEvent(UIType.UIEquipment, (int)UISortingOrder.Popup, false)]
    [FriendOf(typeof(UIGlobalComponent))]
    public class UIEquipmentEvent : AUIEvent
    {
        public override async ETTask<UI> OnCreate(UIComponent uiComponent)
        {
            string assetsName = $"Assets/Bundles/UI/UIEquipment/{UIType.UIEquipment}.prefab";
            GameObject bundleGameObject = await uiComponent.Scene().GetComponent<ResourcesLoaderComponent>()
                .LoadAssetAsync<GameObject>(assetsName);
            GameObject gameObject = UnityEngine.Object.Instantiate(bundleGameObject, uiComponent.UIGlobalComponent.UICanvas);
            gameObject.SetActive(true);
            UI ui = uiComponent.AddChild<UI, string, GameObject, int>(UIType.UIEquipment, gameObject,
                UIEventComponent.Instance.UISortingOrders[UIType.UIEquipment]);
            ui.AddComponent<UIEquipmentComponent>();
            return ui;
        }

        public override void OnRemove(UIComponent uiComponent)
        {
        }
    }
}