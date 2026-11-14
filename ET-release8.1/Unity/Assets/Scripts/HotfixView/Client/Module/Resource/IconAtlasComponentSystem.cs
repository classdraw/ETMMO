using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;
using YooAsset;

namespace ET.Client
{
    [EntitySystemOf(typeof(IconAtlasComponent))]
    [FriendOf(typeof(IconAtlasComponent))]
    [FriendOf(typeof(ResourcesLoaderComponent))]
    public static partial class IconAtlasComponentSystem
    {
        private const string SubAssetsHandlePrefix = "SubAssets|";

        [EntitySystem]
        private static void Awake(this IconAtlasComponent self)
        {
            self.SpriteCache = new Dictionary<string, Sprite>();
        }

        [EntitySystem]
        private static void Destroy(this IconAtlasComponent self)
        {
            // 会话级：切地图不 Release 句柄，退出进程时由 Root ResourcesLoaderComponent.Destroy 统一释放
            self.SpriteCache = null;
            self.SpritesIndexed = false;
            self.Indexing = false;
            self.LoadFailed = false;
        }

        public static IconAtlasComponent GetIconAtlas(this Scene scene)
        {
            return scene?.Root()?.GetComponent<IconAtlasComponent>();
        }

        public static IconAtlasComponent GetIconAtlas(this Entity entity)
        {
            return entity?.Root()?.GetComponent<IconAtlasComponent>();
        }

        public static IconAtlasComponent GetIconAtlas(this UI ui)
        {
            return ((Entity)ui).GetIconAtlas();
        }

        /// <summary>从 Root 会话图集缓存取 Sprite（Loading 阶段已索引）。</summary>
        public static bool TryGetIconSprite(this Entity entity, string spriteName, out Sprite sprite)
        {
            sprite = null;
            IconAtlasComponent atlas = entity.GetIconAtlas();
            return atlas != null && atlas.TryGetSprite(spriteName, out sprite);
        }

        public static bool TryGetSprite(this IconAtlasComponent self, string spriteName, out Sprite sprite)
        {
            sprite = null;
            if (self == null || self.SpriteCache == null || string.IsNullOrEmpty(spriteName))
            {
                return false;
            }

            if (self.SpriteCache.TryGetValue(spriteName, out sprite))
            {
                return true;
            }

            foreach (KeyValuePair<string, Sprite> pair in self.SpriteCache)
            {
                if (string.Equals(pair.Key, spriteName, StringComparison.OrdinalIgnoreCase))
                {
                    sprite = pair.Value;
                    self.SpriteCache[spriteName] = sprite;
                    return true;
                }
            }

            return false;
        }

        public static async ETTask EnsureIndexedAsync(this IconAtlasComponent self, IEnumerable<string> subAssetFallbackNames = null)
        {
            if (self == null || self.IsDisposed)
            {
                return;
            }

            if (self.SpritesIndexed || self.LoadFailed)
            {
                return;
            }

            if (self.Indexing)
            {
                await WaitIndexingAsync(self);
                return;
            }

            self.Indexing = true;
            try
            {
                ResourcesLoaderComponent loader = GetResourcesLoader(self);
                if (loader == null)
                {
                    Log.Error($"[{IconAtlasComponent.LogTag}] ResourcesLoaderComponent 为空");
                    self.LoadFailed = true;
                    return;
                }

                if (self.SpriteCache == null)
                {
                    self.SpriteCache = new Dictionary<string, Sprite>();
                }

                string atlasLocation = ResolveAtlasLocation(
                    loader.package, IconAtlasComponent.AtlasLocation, IconAtlasComponent.AtlasShortLocation);
                if (string.IsNullOrEmpty(atlasLocation))
                {
                    Log.Warning($"[{IconAtlasComponent.LogTag}] 图集未收录：{IconAtlasComponent.AtlasLocation}，将尝试单张路径");
                    self.SpritesIndexed = true;
                    return;
                }

                if (self.SpritesIndexed && self.SpriteCache.Count == 0)
                {
                    self.SpritesIndexed = false;
                }

                Scene root = self.Root();
                Log.Info($"[{IconAtlasComponent.LogTag}] 开始批量索引 location={atlasLocation}");
                int countBefore = self.SpriteCache.Count;

                using (CoroutineLock coroutineLock = await root.GetComponent<CoroutineLockComponent>()
                           .Wait(CoroutineLockType.ResourcesLoader, atlasLocation.GetHashCode()))
                {
                    SubAssetsHandle subAssetsHandle = await GetOrCreateSubAssetsHandle(loader, atlasLocation);
                    if (subAssetsHandle != null)
                    {
                        IndexFromSubAssetsHandle(self.SpriteCache, subAssetsHandle, subAssetFallbackNames);
                        Sprite[] subSprites = subAssetsHandle.GetSubAssetObjects<Sprite>();
                        Log.Info(
                            $"[{IconAtlasComponent.LogTag}] LoadSubAssets 缓存={self.SpriteCache.Count} SubAsset(Sprite)={subSprites?.Length ?? 0}");
                    }
                }

                if (self.SpriteCache.Count <= countBefore)
                {
                    await IndexFromAllAssetsAsync(self.SpriteCache, loader, atlasLocation);
                }

                if (self.SpriteCache.Count <= countBefore)
                {
                    await IndexFromSpriteAtlasAssetAsync(self.SpriteCache, loader, atlasLocation);
                }

                if (self.SpriteCache.Count > countBefore)
                {
                    self.SpritesIndexed = true;
                    Log.Info($"[{IconAtlasComponent.LogTag}] 批量索引成功，Sprite 数={self.SpriteCache.Count}");
                }
                else
                {
                    Log.Warning(
                        $"[{IconAtlasComponent.LogTag}] 批量索引为 0，保留未索引状态以便重试/单张加载：{IconAtlasComponent.SingleSpriteFolder}/{{name}}.png");
                }
            }
            finally
            {
                if (self != null && !self.IsDisposed)
                {
                    self.Indexing = false;
                }
            }
        }

        private static async ETTask WaitIndexingAsync(IconAtlasComponent self)
        {
            if (self == null || self.IsDisposed || !self.Indexing)
            {
                return;
            }

            TimerComponent timer = self.Root().GetComponent<TimerComponent>();
            if (timer == null)
            {
                return;
            }

            while (self.Indexing && !self.IsDisposed)
            {
                await timer.WaitFrameAsync();
            }
        }

        public static async ETTask<Sprite> GetSpriteAsync(
            this IconAtlasComponent self,
            string spriteName,
            IEnumerable<string> subAssetFallbackNames = null)
        {
            if (self == null || self.IsDisposed)
            {
                return null;
            }

            if (self.TryGetSprite(spriteName, out Sprite cached))
            {
                return cached;
            }

            if (self.LoadFailed)
            {
                return null;
            }

            await self.EnsureIndexedAsync(subAssetFallbackNames);

            if (self.TryGetSprite(spriteName, out cached))
            {
                return cached;
            }

            ResourcesLoaderComponent loader = GetResourcesLoader(self);
            return await LoadSingleSpriteAsync(loader, self.SpriteCache, spriteName);
        }

        /// <summary>物品表 Icon 名，作 SubAssets 索引补充。</summary>
        public static IEnumerable<string> CollectItemConfigIconNames()
        {
            foreach (KeyValuePair<int, ItemConfig> pair in ItemConfigCategory.Instance.GetAll())
            {
                ItemConfig config = pair.Value;
                if (config != null && !string.IsNullOrEmpty(config.Icon))
                {
                    yield return config.Icon;
                }
            }
        }

        private static ResourcesLoaderComponent GetResourcesLoader(IconAtlasComponent self)
        {
            return self?.Root()?.GetComponent<ResourcesLoaderComponent>();
        }

        private static string ResolveAtlasLocation(ResourcePackage package, string atlasLocation, string shortAtlasLocation)
        {
            if (package == null || string.IsNullOrEmpty(atlasLocation))
            {
                return null;
            }

            if (package.CheckLocationValid(atlasLocation))
            {
                return atlasLocation;
            }

            if (!string.IsNullOrEmpty(shortAtlasLocation) && package.CheckLocationValid(shortAtlasLocation))
            {
                return shortAtlasLocation;
            }

            return null;
        }

        private static async ETTask<Sprite> LoadSingleSpriteAsync(
            ResourcesLoaderComponent loader,
            Dictionary<string, Sprite> cache,
            string spriteName)
        {
            if (loader?.package == null || cache == null || string.IsNullOrEmpty(spriteName))
            {
                return null;
            }

            string location = $"{IconAtlasComponent.SingleSpriteFolder}/{spriteName}.png";
            if (!loader.package.CheckLocationValid(location))
            {
                Log.Warning($"[{IconAtlasComponent.LogTag}] 单张 Sprite 不在包内 location={location}");
                return null;
            }

            Sprite sprite = await loader.LoadAssetAsync<Sprite>(location);
            if (sprite != null)
            {
                cache[spriteName] = sprite;
            }

            return sprite;
        }

        private static void IndexFromSubAssetsHandle(
            Dictionary<string, Sprite> cache,
            SubAssetsHandle subAssetsHandle,
            IEnumerable<string> fallbackNames)
        {
            foreach (Sprite sprite in subAssetsHandle.GetSubAssetObjects<Sprite>())
            {
                if (sprite != null)
                {
                    cache[sprite.name] = sprite;
                }
            }

            if (cache.Count > 0 || fallbackNames == null)
            {
                return;
            }

            foreach (string name in fallbackNames)
            {
                if (string.IsNullOrEmpty(name) || cache.ContainsKey(name))
                {
                    continue;
                }

                Sprite sprite = subAssetsHandle.GetSubAssetObject<Sprite>(name);
                if (sprite != null)
                {
                    cache[name] = sprite;
                }
            }
        }

        private static async ETTask IndexFromAllAssetsAsync(
            Dictionary<string, Sprite> cache,
            ResourcesLoaderComponent loader,
            string atlasLocation)
        {
            Dictionary<string, Sprite> sprites = await loader.LoadAllAssetsAsync<Sprite>(atlasLocation);
            if (sprites == null || sprites.Count == 0)
            {
                return;
            }

            foreach (KeyValuePair<string, Sprite> pair in sprites)
            {
                if (pair.Value != null)
                {
                    cache[pair.Key] = pair.Value;
                }
            }
        }

        private static async ETTask IndexFromSpriteAtlasAssetAsync(
            Dictionary<string, Sprite> cache,
            ResourcesLoaderComponent loader,
            string atlasLocation)
        {
            SpriteAtlas atlas = await loader.LoadAssetAsync<SpriteAtlas>(atlasLocation);
            if (atlas == null || atlas.spriteCount <= 0)
            {
                Log.Warning($"[{IconAtlasComponent.LogTag}] LoadAssetAsync<SpriteAtlas> 无子图 location={atlasLocation}");
                return;
            }

            Sprite[] sprites = new Sprite[atlas.spriteCount];
            atlas.GetSprites(sprites);
            int n = 0;
            foreach (Sprite sprite in sprites)
            {
                if (sprite == null)
                {
                    continue;
                }

                cache[sprite.name] = sprite;
                n++;
            }

            Log.Info($"[{IconAtlasComponent.LogTag}] SpriteAtlas.GetSprites 写入={n}");
        }

        private static async ETTask<SubAssetsHandle> GetOrCreateSubAssetsHandle(ResourcesLoaderComponent loader, string atlasLocation)
        {
            string handleKey = SubAssetsHandlePrefix + atlasLocation;
            if (loader.handlers.TryGetValue(handleKey, out HandleBase existing) && existing is SubAssetsHandle cached)
            {
                return cached;
            }

            if (loader.package == null)
            {
                Log.Warning($"[{IconAtlasComponent.LogTag}] LoadSubAssetsAsync 失败：DefaultPackage 为空");
                return null;
            }

            SubAssetsHandle handle = loader.package.LoadSubAssetsAsync<Sprite>(atlasLocation);
            await handle.Task;
            if (handle.Status != EOperationStatus.Succeed)
            {
                Log.Warning($"[{IconAtlasComponent.LogTag}] LoadSubAssetsAsync 失败 location={atlasLocation} error={handle.LastError}");
                return null;
            }

            if (!loader.handlers.ContainsKey(handleKey))
            {
                loader.handlers.Add(handleKey, handle);
            }

            return handle;
        }
    }
}
