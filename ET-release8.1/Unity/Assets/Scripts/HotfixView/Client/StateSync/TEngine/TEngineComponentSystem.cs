using TEngine;
using UnityEngine;
using I2Localization = TEngine.Localization.LocalizationManager;

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
            ClientViewDebugSettings.TryApplyRootModuleLanguage(gameEntry);
            self.EngineGlobal = gameEntry.GetComponent<TEngineGlobal>();
            // 先绑定 YooAsset 再 Start，LocalizationManager.Awake 需要 IResourceModuleET
            self.EngineGlobal.SetResAgent(resLoader.ResourceAgent);
            await self.EngineGlobal.StartEngine();//框架初始化
            await self.ApplyInitRuntimeLanguageAfterLocalizationReady();
            
            //ModuleSystem.GetModule<IResourceModuleET>()
            self.LoadSetting();
        }

        private const int InitLanguageWaitMaxFrames = 600;

        /// <summary>等待 I2 语言源就绪后再应用 Init 场景语言，避免 ModuleSystem 提前创建空 LocalizationModule。</summary>
        private static async ETTask ApplyInitRuntimeLanguageAfterLocalizationReady(this TEngineComponent self)
        {
            if (ClientViewDebugSettings.RuntimeLanguage == Language.Unspecified || self.GameEntryObj == null)
            {
                return;
            }

            LocalizationManager localizationManager = self.GameEntryObj.GetComponentInChildren<LocalizationManager>(true);
            if (localizationManager == null)
            {
                Log.Warning("Init 运行语言未应用：GameEntry 上未找到 LocalizationManager。");
                return;
            }

            TimerComponent timer = self.Root().GetComponent<TimerComponent>();
            const string probeTerm = UILocalizationHelper.EquipmentLevelTerm;
            for (int i = 0; i < InitLanguageWaitMaxFrames; i++)
            {
                if (I2Localization.Sources != null && I2Localization.Sources.Count > 0)
                {
                    UILocalizationHelper.EnsureLocalizationReadyForInit(out _, out _);
                    ClientViewDebugSettings.TryApplyOnLocalizationManager(localizationManager, load: false);
                    ClientViewDebugSettings.TryForceI2CurrentLanguage();
                    if (UILocalizationHelper.IsTermResolvable(probeTerm))
                    {
                        return;
                    }
                }

                await timer.WaitFrameAsync();
            }

            Log.Warning("Init 运行语言未能应用：等待语言表/词条就绪超时。");
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
            if (self.PostProcessingObj != null)
            {
                UnityEngine.Object.Destroy(self.PostProcessingObj);
                self.PostProcessingObj = null;
            }

            if (self.GameEntryObj != null)
            {
                GameObject.Destroy(self.GameEntryObj);
                self.GameEntryObj = null;
            }
        }

        /// <summary>Resources.LoadAsync 加载后效 prefab（路径见 TEngineComponent.PostProcessingResourcesPath）</summary>
        public static async ETTask LoadPostProcessingPrefabAsync(this TEngineComponent self)
        {
            if (self.PostProcessingObj != null)
            {
                return;
            }

            ResourceRequest request = Resources.LoadAsync<GameObject>(TEngineComponent.PostProcessingResourcesPath);
            TimerComponent timer = self.Root().GetComponent<TimerComponent>();
            while (!request.isDone)
            {
                await timer.WaitFrameAsync();
            }

            GameObject prefab = request.asset as GameObject;
            if (prefab == null)
            {
                Log.Error($"Resources 未找到后效 prefab: {TEngineComponent.PostProcessingResourcesPath}");
                return;
            }

            GameObject instance = UnityEngine.Object.Instantiate(prefab);
            instance.name = "PostProcessing";
            UnityEngine.Object.DontDestroyOnLoad(instance);
            if (self.GameEntryObj != null)
            {
                instance.transform.SetParent(self.GameEntryObj.transform, false);
            }

            self.PostProcessingObj = instance;
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
