using System;
using UnityEngine;

namespace ET.Client
{
    [EntitySystemOf(typeof(UIGlobalComponent))]
    [FriendOf(typeof(UIGlobalComponent))]
    [FriendOfAttribute(typeof(ET.Client.TEngineComponent))]
    public static partial class UIGlobalComponentSystem
    {
        [EntitySystem]
        public static void Awake(this UIGlobalComponent self)
        {

            //uiRoot初始化
            var gameEntryObj = self.Root().GetComponent<TEngineComponent>().GameEntryObj;
            self.UIRootObj = gameEntryObj.transform.Find("UIRoot");
            self.UICanvas=gameEntryObj.transform.Find("UIRoot/UICanvas");
            //GameObject uiRoot = GameObject.Find("/Global/UI");
            //ReferenceCollector referenceCollector = uiRoot.GetComponent<ReferenceCollector>();

            //self.UILayers.Add((int)UILayer.Hidden, referenceCollector.Get<GameObject>(UILayer.Hidden.ToString()).transform);
            //self.UILayers.Add((int)UILayer.Low, referenceCollector.Get<GameObject>(UILayer.Low.ToString()).transform);
            //self.UILayers.Add((int)UILayer.Mid, referenceCollector.Get<GameObject>(UILayer.Mid.ToString()).transform);
            //self.UILayers.Add((int)UILayer.High, referenceCollector.Get<GameObject>(UILayer.High.ToString()).transform);
        }
        [EntitySystem]
        public static void Destroy(this UIGlobalComponent self)
        {
            self.UIRootObj = null;
            self.UICanvas = null;
        }
        private static async ETTask<GameObject> LoadGameObjectInstance(ResourcesLoaderComponent resLoader, string location)
        {
            var bundleGameObject = await resLoader.LoadAssetAsync<GameObject>(location);
            GameObject gameObject = UnityEngine.Object.Instantiate(bundleGameObject) as GameObject;
            return gameObject;

        }

        public static async ETTask<UI> OnCreate(this UIGlobalComponent self, UIComponent uiComponent, string uiType)
        {
            try
            {
                UI ui = await UIEventComponent.Instance.UIEvents[uiType].OnCreate(uiComponent);
                return ui;
            }
            catch (Exception e)
            {
                throw new Exception($"on create ui error: {uiType}", e);
            }
        }
        /*
                public static Transform GetLayer(this UIGlobalComponent self, int layer)
                {
                    return self.UILayers[layer];
                }
        */
        public static void OnRemove(this UIGlobalComponent self, UIComponent uiComponent, string uiType)
        {
            try
            {
                UIEventComponent.Instance.UIEvents[uiType].OnRemove(uiComponent);
            }
            catch (Exception e)
            {
                throw new Exception($"on remove ui error: {uiType}", e);
            }
        }
    }
}