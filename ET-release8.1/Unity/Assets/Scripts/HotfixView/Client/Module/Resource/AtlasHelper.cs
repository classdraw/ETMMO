using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;
using YooAsset;

namespace ET.Client
{
    /// <summary>图集加载与 Sprite 缓存（HotfixView，状态挂在具体 Component 字段上）。</summary>
    [FriendOf(typeof(ResourcesLoaderComponent))]
    [FriendOf(typeof(UIBagComponent))]
    public static class AtlasHelper
    {
        private const string SubAssetsHandlePrefix = "SubAssets|";
        private const string UIBagItemIconLogTag = "ItemIconAtlas";

        public static bool TryGetSprite(Dictionary<string, Sprite> cache, string spriteName, out Sprite sprite)
        {
            sprite = null;
            if (cache == null || string.IsNullOrEmpty(spriteName))
            {
                return false;
            }

            if (cache.TryGetValue(spriteName, out sprite))
            {
                return true;
            }

            foreach (KeyValuePair<string, Sprite> pair in cache)
            {
                if (string.Equals(pair.Key, spriteName, StringComparison.OrdinalIgnoreCase))
                {
                    sprite = pair.Value;
                    cache[spriteName] = sprite;
                    return true;
                }
            }

            return false;
        }

        public static string ResolveAtlasLocation(ResourcePackage package, string atlasLocation, string shortAtlasLocation)
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

        public static async ETTask EnsureItemIconAtlasIndexedAsync(
            UIBagComponent bag,
            Scene root,
            ResourcesLoaderComponent loader,
            IEnumerable<string> subAssetFallbackNames)
        {
            if (bag == null)
            {
                return;
            }

            if (bag.ItemIconAtlasSpritesIndexed || bag.ItemIconAtlasLoadFailed || bag.ItemIconAtlasIndexing)
            {
                return;
            }

            bag.ItemIconAtlasIndexing = true;
            try
            {
                if (loader == null)
                {
                    Log.Error($"[{UIBagItemIconLogTag}] ResourcesLoaderComponent 为空");
                    bag.ItemIconAtlasLoadFailed = true;
                    return;
                }

                if (bag.IconSpriteCache == null)
                {
                    bag.IconSpriteCache = new Dictionary<string, Sprite>();
                }

                string atlasLocation = ResolveAtlasLocation(
                    loader.package, UIBagComponent.ItemIconAtlasLocation, UIBagComponent.ItemIconAtlasShortLocation);
                if (string.IsNullOrEmpty(atlasLocation))
                {
                    Log.Warning($"[{UIBagItemIconLogTag}] 图集未收录：{UIBagComponent.ItemIconAtlasLocation}，将尝试单张路径");
                    bag.ItemIconAtlasSpritesIndexed = true;
                    return;
                }

                Log.Info($"[{UIBagItemIconLogTag}] 开始批量索引 location={atlasLocation}");
                int countBefore = bag.IconSpriteCache.Count;

                using (CoroutineLock coroutineLock = await root.GetComponent<CoroutineLockComponent>()
                           .Wait(CoroutineLockType.ResourcesLoader, atlasLocation.GetHashCode()))
                {
                    SubAssetsHandle subAssetsHandle = await GetOrCreateSubAssetsHandle(loader, atlasLocation, UIBagItemIconLogTag);
                    if (subAssetsHandle != null)
                    {
                        IndexFromSubAssetsHandle(bag.IconSpriteCache, subAssetsHandle, subAssetFallbackNames);
                        Sprite[] subSprites = subAssetsHandle.GetSubAssetObjects<Sprite>();
                        Log.Info(
                            $"[{UIBagItemIconLogTag}] LoadSubAssets 缓存={bag.IconSpriteCache.Count} SubAsset(Sprite)={subSprites?.Length ?? 0}");
                    }
                }

                if (bag.IconSpriteCache.Count <= countBefore)
                {
                    await IndexFromAllAssetsAsync(bag.IconSpriteCache, loader, atlasLocation);
                }

                if (bag.IconSpriteCache.Count <= countBefore)
                {
                    await IndexFromSpriteAtlasAssetAsync(bag.IconSpriteCache, loader, atlasLocation, UIBagItemIconLogTag);
                }

                bag.ItemIconAtlasSpritesIndexed = true;
                if (bag.IconSpriteCache.Count > countBefore)
                {
                    Log.Info($"[{UIBagItemIconLogTag}] 批量索引成功，Sprite 数={bag.IconSpriteCache.Count}");
                }
                else
                {
                    Log.Warning(
                        $"[{UIBagItemIconLogTag}] 批量索引为 0，将单张加载：{UIBagComponent.ItemIconSingleFolder}/{{name}}.png");
                }
            }
            finally
            {
                if (bag != null && !bag.IsDisposed)
                {
                    bag.ItemIconAtlasIndexing = false;
                }
            }
        }

        public static async ETTask<Sprite> GetItemIconSpriteAsync(
            UIBagComponent bag,
            Scene root,
            ResourcesLoaderComponent loader,
            string spriteName,
            IEnumerable<string> subAssetFallbackNames)
        {
            if (bag?.IconSpriteCache != null && TryGetSprite(bag.IconSpriteCache, spriteName, out Sprite cached))
            {
                return cached;
            }

            if (bag == null || bag.ItemIconAtlasLoadFailed)
            {
                return null;
            }

            await EnsureItemIconAtlasIndexedAsync(bag, root, loader, subAssetFallbackNames);

            if (bag.IconSpriteCache != null && TryGetSprite(bag.IconSpriteCache, spriteName, out cached))
            {
                return cached;
            }

            return await LoadSingleSpriteAsync(
                loader, bag.IconSpriteCache, UIBagComponent.ItemIconSingleFolder, UIBagItemIconLogTag, spriteName);
        }

        private static async ETTask<Sprite> LoadSingleSpriteAsync(
            ResourcesLoaderComponent loader,
            Dictionary<string, Sprite> cache,
            string singleSpriteFolder,
            string logTag,
            string spriteName)
        {
            if (loader?.package == null || cache == null || string.IsNullOrEmpty(spriteName) ||
                string.IsNullOrEmpty(singleSpriteFolder))
            {
                return null;
            }

            string location = $"{singleSpriteFolder}/{spriteName}.png";
            if (!loader.package.CheckLocationValid(location))
            {
                Log.Warning($"[{logTag}] 单张 Sprite 不在包内 location={location}");
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
            string atlasLocation,
            string logTag)
        {
            SpriteAtlas atlas = await loader.LoadAssetAsync<SpriteAtlas>(atlasLocation);
            if (atlas == null || atlas.spriteCount <= 0)
            {
                Log.Warning($"[{logTag}] LoadAssetAsync<SpriteAtlas> 无子图 location={atlasLocation}");
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

            Log.Info($"[{logTag}] SpriteAtlas.GetSprites 写入={n}");
        }

        private static async ETTask<SubAssetsHandle> GetOrCreateSubAssetsHandle(
            ResourcesLoaderComponent loader,
            string atlasLocation,
            string logTag)
        {
            string handleKey = SubAssetsHandlePrefix + atlasLocation;
            if (loader.handlers.TryGetValue(handleKey, out HandleBase existing) && existing is SubAssetsHandle cached)
            {
                return cached;
            }

            if (loader.package == null)
            {
                Log.Warning($"[{logTag}] LoadSubAssetsAsync 失败：DefaultPackage 为空");
                return null;
            }

            SubAssetsHandle handle = loader.package.LoadSubAssetsAsync<Sprite>(atlasLocation);
            await handle.Task;
            if (handle.Status != EOperationStatus.Succeed)
            {
                Log.Warning($"[{logTag}] LoadSubAssetsAsync 失败 location={atlasLocation} error={handle.LastError}");
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
