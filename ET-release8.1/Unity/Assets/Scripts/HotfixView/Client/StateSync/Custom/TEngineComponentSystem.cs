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
            //框架必须要加载的东西丢这里
            var resLoader = self.Root().GetComponent<ResourcesLoaderComponent>();
            
            GameObject gameEntry = await LoadGameObjectInstance(resLoader, self.GameEntryPath);
            GameObject.DontDestroyOnLoad(gameEntry);
            self.GameEntryObj = gameEntry;
            self.EngineGlobal = gameEntry.GetComponent<TEngineGlobal>();
            await self.EngineGlobal.StartEngine();//框架初始化
            self.EngineGlobal.SetResAgent(resLoader.ResourceAgent);//绑定资源加载器
            
            //ModuleSystem.GetModule<IResourceModuleET>()

        }

        private static async ETTask<GameObject> LoadGameObjectInstance(ResourcesLoaderComponent resLoader,string location)
        {
            var bundleGameObject = await resLoader.LoadAssetAsync<GameObject>(location);
            GameObject gameObject = UnityEngine.Object.Instantiate(bundleGameObject) as GameObject;
            return gameObject;

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