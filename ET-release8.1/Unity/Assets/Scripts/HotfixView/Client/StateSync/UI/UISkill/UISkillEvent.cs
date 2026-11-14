using UnityEngine;

namespace ET.Client
{
    [UIEvent(UIType.UISkill, (int)UISortingOrder.Popup, false)]
    [FriendOfAttribute(typeof(ET.Client.UIGlobalComponent))]
    public class UISkillEvent : AUIEvent
    {
        public override async ETTask<UI> OnCreate(UIComponent uiComponent)
        {
            string assetsName = $"Assets/Bundles/UI/UISkill/{UIType.UISkill}.prefab";
            GameObject bundleGameObject = await uiComponent.Scene().GetComponent<ResourcesLoaderComponent>()
                .LoadAssetAsync<GameObject>(assetsName);
            GameObject gameObject = UnityEngine.Object.Instantiate(bundleGameObject, uiComponent.UIGlobalComponent.UICanvas);
            gameObject.SetActive(true);
            UI ui = uiComponent.AddChild<UI, string, GameObject, int>(UIType.UISkill, gameObject,
                UIEventComponent.Instance.UISortingOrders[UIType.UISkill]);
            ui.AddComponent<UISkillComponent>();
            return ui;
        }

        public override void OnRemove(UIComponent uiComponent)
        {
        }
    }
}