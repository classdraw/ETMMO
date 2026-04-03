using TEngine;
using UnityEngine;

namespace ET.Client
{
    [EntitySystemOf(typeof(TUIComponent))]
    [FriendOf(typeof(TUIComponent))]
    [FriendOf(typeof(TEngineComponent))]
    public static partial class TUIComponentSystem
    {
        [EntitySystem]
        private static void Awake(this TUIComponent self)
        {
            self.UIRoot=self.Root().GetComponent<TEngineComponent>().GameEntryObj.transform.Find("UIRoot");
            self.UICamera=self.UIRoot.Find("UICamera").GetComponent<Camera>();
            //Log.Info("11111111111111111111111");
        }


    }
}