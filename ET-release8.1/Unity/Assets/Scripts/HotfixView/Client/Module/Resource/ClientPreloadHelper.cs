namespace ET.Client
{
    // <summary>
    /// 客户端启动期预加载（Root 挂载完成后、首次切场景前）。
    /// 在此集中登记会话级资源，避免各 UI / Loading 重复等待。
    /// </summary>
    [FriendOfAttribute(typeof(ET.Client.IconAtlasComponent))]
    public static class ClientPreloadHelper
    {
        public static async ETTask PreloadOnStartupAsync(Scene root)
        {
            if (root == null)
            {
                Log.Error("[ClientPreload] Root 为空");
                return;
            }

            await PreloadIconAtlasAsync(root);
        }

        private static async ETTask PreloadIconAtlasAsync(Scene root)
        {
            IconAtlasComponent atlas = root.GetComponent<IconAtlasComponent>();
            if (atlas == null)
            {
                Log.Error($"[{IconAtlasComponent.LogTag}] Root 未挂载 IconAtlasComponent");
                return;
            }

            if (atlas.SpritesIndexed || atlas.LoadFailed)
            {
                return;
            }

            Log.Info($"[{IconAtlasComponent.LogTag}] 启动预加载图集");
            await atlas.EnsureIndexedAsync(IconAtlasComponentSystem.CollectItemConfigIconNames());
            int count = atlas.SpriteCache?.Count ?? 0;
            if (count > 0)
            {
                Log.Info($"[{IconAtlasComponent.LogTag}] 启动预加载完成，Sprite 数={count}");
            }
            else
            {
                Log.Warning($"[{IconAtlasComponent.LogTag}] 启动预加载未得到 Sprite（资源未就绪将稍后重试）");
            }
        }
    }
}