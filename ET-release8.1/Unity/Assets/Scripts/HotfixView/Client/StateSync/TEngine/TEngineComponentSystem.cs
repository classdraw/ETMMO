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

        public static async ETTask Init(this TEngineComponent self)
        {
            //框架必须要加载的东西丢这里
            var resLoader = self.Root().GetComponent<ResourcesLoaderComponent>();
            
            GameObject gameEntry = await LoadGameObjectInstance(resLoader, self.GameEntryPath);
            GameObject.DontDestroyOnLoad(gameEntry);
            gameEntry.name = "TEngineComponent(Object)";
            self.GameEntryObj = gameEntry;
            self.EngineGlobal = gameEntry.GetComponent<TEngineGlobal>();
            await self.EngineGlobal.StartEngine();//框架初始化
            self.EngineGlobal.SetResAgent(resLoader.ResourceAgent);//绑定资源加载器
            
            //ModuleSystem.GetModule<IResourceModuleET>()
            self.LoadSetting();
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


        private static void LoadSetting(this TEngineComponent self)
        {
            self.SettingValues.Clear();
            foreach (Setting_Key_Enum key in SettingHelper.GetAllKeys())
            {
                SettingHelper.EnsureDefault(key);
                self.SettingValues[key] = SettingHelper.LoadFloat(key);
            }

            self.RefreshSetting();
        }

        public static void RefreshSetting(this TEngineComponent self)
        {
            //VolumeManager.
            /***
            bool enablePostProcessing = self.SettingValues.TryGetValue(Setting_Key_Enum.PostProcessingKey, out float cacheValue)
                ? cacheValue >= 0.5f
                : SettingHelper.LoadBool(Setting_Key_Enum.PostProcessingKey);

            PostProcessVolume[] volumes = UnityEngine.Object.FindObjectsOfType<PostProcessVolume>(true);
            foreach (PostProcessVolume volume in volumes)
            {
                volume.enabled = enablePostProcessing;
            }

            PostProcessLayer[] layers = UnityEngine.Object.FindObjectsOfType<PostProcessLayer>(true);
            foreach (PostProcessLayer layer in layers)
            {
                layer.enabled = enablePostProcessing;
            }*/
        }

        public static void SaveSetting(this TEngineComponent self, Setting_Key_Enum key, object obj)
        {
            if (!SettingHelper.TrySave(key, obj, out float cacheValue))
            {
                return;
            }

            self.SettingValues[key] = cacheValue;
            self.RefreshSetting();
        }

    }

}