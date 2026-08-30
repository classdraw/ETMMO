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
            ResourcesLoaderComponent resLoader = self.Root().GetComponent<ResourcesLoaderComponent>();
            self.RoleTextureConfigs.Clear();

            for (int i = (int)FrameRolePartType.Body; i < (int)FrameRolePartType.Count; i++)
            {
                FrameRolePartType part = (FrameRolePartType)i;
                string location = BuildRoleTextureLocation(self, part);
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
