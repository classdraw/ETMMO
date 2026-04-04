using System.Collections.Generic;
using TEngine;
using UnityEngine.SceneManagement;
using YooAsset;

namespace ET.Client
{
    [EntitySystemOf(typeof(ResourcesLoaderComponent))]
    [FriendOf(typeof(ResourcesLoaderComponent))]
    public static partial class ResourcesLoaderComponentSystem
    {
        [EntitySystem]
        private static void Awake(this ResourcesLoaderComponent self)
        {
            self.package = YooAssets.TryGetPackage("DefaultPackage");
            var resAgent=new ResourceAgent();
            resAgent.Init("DefaultPackage",self);
            self.ResourceAgent = resAgent;
        }

        [EntitySystem]
        private static void Awake(this ResourcesLoaderComponent self, string packageName)
        {
            self.package = YooAssets.TryGetPackage(packageName);
            var resAgent = new ResourceAgent();
            resAgent.Init(packageName,self);
            self.ResourceAgent = resAgent;
        }

        [EntitySystem]
        private static void Destroy(this ResourcesLoaderComponent self)
        {
            foreach (var kv in self.handlers)
            {
                if (kv.Value != null && kv.Value.IsValid)
                {
                    switch (kv.Value)
                    {
                        case AssetHandle handle:
                            handle.Release();
                            break;
                        case AllAssetsHandle handle:
                            handle.Release();
                            break;
                        case SubAssetsHandle handle:
                            handle.Release();
                            break;
                        case RawFileHandle handle:
                            handle.Release();
                            break;
                        case SceneHandle handle:
                            if (!handle.IsMainScene())
                            {
                                handle.UnloadAsync();
                            }
                            break;
                    }
                }
            }
            if (self.ResourceAgent!=null) { 
                ((ResourceAgent)self.ResourceAgent).Shutdown();
            }
            self.ResourceAgent = null;
            
        }

        public static async ETTask<T> LoadAssetAsync<T>(this ResourcesLoaderComponent self, string location) where T : UnityEngine.Object
        {
            if (self.ResourceAgent != null)
            {
                return await self.ResourceAgent.LoadAssetAsync<T>(location);
            }

            // 原实现（已注释）
            /*
            using CoroutineLock coroutineLock = await self.Root().GetComponent<CoroutineLockComponent>().Wait(CoroutineLockType.ResourcesLoader, location.GetHashCode());

            HandleBase handler;
            if (!self.handlers.TryGetValue(location, out handler))
            {
                handler = self.package.LoadAssetAsync<T>(location);

                await handler.Task;

                self.handlers.Add(location, handler);
            }

            return (T)((AssetHandle)handler).AssetObject;
            */
            return null;
        }


        /// <summary>
        /// 这个屌方法少用，只有部分框架性的东西需要用到，游戏内尽量边玩边下
        /// </summary>
        public static T LoadAssetSync<T>(this ResourcesLoaderComponent self, string location) where T : UnityEngine.Object
        {

            if (self.ResourceAgent != null)
            {
                return self.ResourceAgent.LoadAsset<T>(location);
            }

            // 原实现（已注释）
            /*
            lock (self.handlers)
            {
                HandleBase handler;
                if (!self.handlers.TryGetValue(location, out handler))
                {
                    handler = self.package.LoadAssetSync<T>(location);
                    self.handlers.Add(location, handler);
                }

                return (T)((AssetHandle)handler).AssetObject;
            }
            */
            return null;
        }

        public static async ETTask<Dictionary<string, T>> LoadAllAssetsAsync<T>(this ResourcesLoaderComponent self, string location) where T : UnityEngine.Object
        {
            if (self.ResourceAgent != null)
            {
                return await self.ResourceAgent.LoadAllAssetsAsync<T>(location);
            }
            // 原实现（已注释）
            /*
            using CoroutineLock coroutineLock = await self.Root().GetComponent<CoroutineLockComponent>().Wait(CoroutineLockType.ResourcesLoader, location.GetHashCode());

            HandleBase handler;
            if (!self.handlers.TryGetValue(location, out handler))
            {
                handler = self.package.LoadAllAssetsAsync<T>(location);
                await handler.Task;
                self.handlers.Add(location, handler);
            }

            Dictionary<string, T> dictionary = new Dictionary<string, T>();
            foreach (UnityEngine.Object assetObj in ((AllAssetsHandle)handler).AllAssetObjects)
            {
                T t = assetObj as T;
                dictionary.Add(t.name, t);
            }

            return dictionary;
            */
            return new Dictionary<string, T>();
        }

        public static async ETTask LoadSceneAsync(this ResourcesLoaderComponent self, string location, LoadSceneMode loadSceneMode)
        {
            if (self.ResourceAgent != null)
            {
                await self.ResourceAgent.LoadSceneAsync(location, loadSceneMode);
                return;
            }

            // 原实现（已注释）
            /*
            using CoroutineLock coroutineLock = await self.Root().GetComponent<CoroutineLockComponent>().Wait(CoroutineLockType.ResourcesLoader, location.GetHashCode());

            HandleBase handler;
            if (self.handlers.TryGetValue(location, out handler))
            {
                return;
            }

            handler = self.package.LoadSceneAsync(location);

            await handler.Task;
            self.handlers.Add(location, handler);
            */
        }
    }

    /// <summary>
    /// 用来管理资源，生命周期跟随Parent，比如CurrentScene用到的资源应该用CurrentScene的ResourcesLoaderComponent来加载
    /// 这样CurrentScene释放后，它用到的所有资源都释放了
    /// </summary>
    [ComponentOf]
    public class ResourcesLoaderComponent : Entity, IAwake, IAwake<string>, IDestroy
    {
        public ResourcePackage package;
        public Dictionary<string, HandleBase> handlers = new();
        public IResourceModuleET ResourceAgent;//代理
    }
}