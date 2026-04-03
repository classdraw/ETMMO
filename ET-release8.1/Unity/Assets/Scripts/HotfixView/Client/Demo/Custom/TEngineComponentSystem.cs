using TEngine;
using UnityEngine;

namespace ET.Client
{
    [EntitySystemOf(typeof(TEngineComponent))]
    [FriendOf(typeof(TEngineComponent))]
    [FriendOfAttribute(typeof(ET.Client.ResourcesLoaderComponent))]
    public static partial class TEngineComponentSystem
    {
        [EntitySystem]
        private static void Awake(this TEngineComponent self)
        {

        }

        public static async ETTask Init(this TEngineComponent self,ResourcesLoaderComponent resourcesLoaderComponent)
        {
            var resLoader = self.Root().GetComponent<ResourcesLoaderComponent>();
            var bundleGameObject = await resLoader.LoadAssetAsync<GameObject>(self.GameEntryPath);
            GameObject gameObject = UnityEngine.Object.Instantiate(bundleGameObject);
            GameObject.DontDestroyOnLoad(gameObject);
            self.GameEntryObj = gameObject;
            self.EngineGlobal = gameObject.GetComponent<TEngineGlobal>();
            await self.EngineGlobal.StartEngine();//框架初始化
            
            self.EngineGlobal.SetResAgent(resLoader.ResourceAgent);//绑定资源加载器
            
            //ModuleSystem.GetModule<IResourceModuleET>()

        }

        [EntitySystem]
        private static void Destroy(this TEngineComponent self)
        {
            if (self.GameEntryObj != null)
            {
                GameObject.Destroy(self.GameEntryObj);
                self.GameEntryObj = null;
            }
        }

    }
}