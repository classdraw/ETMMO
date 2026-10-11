using YooAsset;

namespace ET.Client
{
    [FriendOf(typeof(RoleTextureComponent))]
    [EntitySystemOf(typeof(RoleTextureComponent))]
    public static partial class RoleTextureComponentSystem
    {
        [EntitySystem]
        private static void Awake(this RoleTextureComponent self)
        {
        }

        [EntitySystem]
        private static void Destroy(this RoleTextureComponent self)
        {
            self.RoleTextureConfigs.Clear();
        }

        public static async ETTask Init(this RoleTextureComponent self)
        {
            self.RoleTextureConfigs.Clear();
            if (!self.LoadConfigOnInit)
            {
                Log.Info("RoleTextureComponent.Init: 已跳过 FrameRoleTexture 配置加载（LoadConfigOnInit=false）");
                await ETTask.CompletedTask;
                return;
            }

            ResourcesLoaderComponent resLoader = self.Root().GetComponent<ResourcesLoaderComponent>();
            ResourcePackage package = resLoader?.package;
            if (package == null)
            {
                Log.Warning("RoleTextureComponent.Init: ResourcesLoaderComponent 或 DefaultPackage 不可用");
                return;
            }

            for (int i = (int)FrameRolePartType.Body; i < (int)FrameRolePartType.Count; i++)
            {
                FrameRolePartType part = (FrameRolePartType)i;
                string location = BuildRoleTextureLocation(self, part);
                if (!package.CheckLocationValid(location))
                {
                    Log.Warning($"RoleTextureConfig 未收录到资源包，跳过: {location}");
                    continue;
                }

                FrameRoleTextureConfig config = await resLoader.LoadAssetAsync<FrameRoleTextureConfig>(location);
                if (config == null)
                {
                    Log.Warning($"RoleTextureConfig 加载失败: {location}");
                    continue;
                }

                config.RebuildDisplayIds();
                config.RebuildLookup();
                self.RoleTextureConfigs[part] = config;
            }
        }

        private static string BuildRoleTextureLocation(RoleTextureComponent self, FrameRolePartType part)
        {
            return $"{self.RoleTextureFolderPath}/{self.RoleTextureFilePrefix}{part}";
        }
    }
}
