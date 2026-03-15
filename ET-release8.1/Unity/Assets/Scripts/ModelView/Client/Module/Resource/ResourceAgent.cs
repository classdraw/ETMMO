using ET;
using ET.Client;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using YooAsset;
using UnityEngine.SceneManagement;

namespace TEngine
{
    [EnableClass]
    [FriendOfAttribute(typeof(ET.Client.ResourcesLoaderComponent))]
    public class ResourceAgent : IResourceModuleET
    {
        private ResourcesLoaderComponent resourcesLoader;
        private ResourcePackage DefaultPackage;

        public string DefaultPackageName { get; set; }
        /// <summary>
        /// 设置异步系统参数，每帧执行消耗的最大时间切片（单位：毫秒）
        /// </summary>
        public long Milliseconds { get; set; } = 30;
        public IResourceModuleET ResAgent { get; set; }

        public void Init(string customPackageName, ResourcesLoaderComponent resourcesLoaderComponent)
        {
            // 初始化资源系统
            YooAssets.Initialize(new ResourceLoggerET());
            YooAssets.SetOperationSystemMaxTimeSlice(Milliseconds);
            // 创建默认的资源包
            DefaultPackage = resourcesLoaderComponent.package;
            DefaultPackageName = customPackageName;
            resourcesLoader = resourcesLoaderComponent;
        }

        public void Shutdown()
        {
            DefaultPackageName = string.Empty;
            DefaultPackage = null;
            resourcesLoader = null;
        }

        public AssetHandle LoadAssetSyncHandle<T>(string location, string packageName = "") where T : UnityEngine.Object
        {
            return LoadAssetSyncHandle(location, typeof(T), packageName);
        }

        public AssetHandle LoadAssetSyncHandle(string location, Type type, string packageName = "")
        {
            ResourcePackage package = string.IsNullOrEmpty(packageName) ? DefaultPackage : YooAssets.GetPackage(packageName);
            if (package == null)
            {
                return null;
            }

            lock (resourcesLoader.handlers)
            {
                if (resourcesLoader.handlers.TryGetValue(location, out HandleBase existingHandler))
                {
                    return existingHandler as AssetHandle;
                }

                AssetHandle handle = package.LoadAssetSync(location, type);
                if (handle != null)
                {
                    resourcesLoader.handlers.Add(location, handle);
                }
                return handle;
            }
        }

        public AssetHandle LoadAssetAsyncHandle<T>(string location, string packageName = "") where T : UnityEngine.Object
        {
            return LoadAssetAsyncHandle(location, typeof(T), packageName);
        }

        public AssetHandle LoadAssetAsyncHandle(string location, Type assetType, string packageName = "")
        {
            ResourcePackage package = string.IsNullOrEmpty(packageName) ? DefaultPackage : YooAssets.GetPackage(packageName);
            if (package == null)
            {
                return null;
            }

            if (resourcesLoader.handlers.TryGetValue(location, out HandleBase existingHandler))
            {
                return existingHandler as AssetHandle;
            }

            AssetHandle handle = package.LoadAssetAsync(location, assetType);
            if (handle != null)
            {
                resourcesLoader.handlers.Add(location, handle);
            }
            return handle;
        }

        public async ETTask LoadAsset<T>(string location, Action<T> callback, string packageName = "") where T : UnityEngine.Object
        {
            if (string.IsNullOrEmpty(location) || callback == null)
            {
                callback?.Invoke(null);
                return;
            }

            using CoroutineLock coroutineLock = await resourcesLoader.Root().GetComponent<CoroutineLockComponent>().Wait(CoroutineLockType.ResourcesLoader, location.GetHashCode());

            HandleBase handler;
            if (!resourcesLoader.handlers.TryGetValue(location, out handler))
            {
                ResourcePackage package = string.IsNullOrEmpty(packageName) ? DefaultPackage : YooAssets.GetPackage(packageName);
                if (package == null)
                {
                    callback?.Invoke(null);
                    return;
                }

                handler = package.LoadAssetAsync<T>(location);
                await handler.Task;
                resourcesLoader.handlers.Add(location, handler);
            }

            T asset = (T)((AssetHandle)handler).AssetObject;
            callback?.Invoke(asset);
        }

        public async ETTask<T> LoadAssetAsync<T>(string location, CancellationToken cancellationToken = default, string packageName = "") where T : UnityEngine.Object
        {
            if (string.IsNullOrEmpty(location))
            {
                return null;
            }

            using CoroutineLock coroutineLock = await resourcesLoader.Root().GetComponent<CoroutineLockComponent>().Wait(CoroutineLockType.ResourcesLoader, location.GetHashCode());

            HandleBase handler;
            if (!resourcesLoader.handlers.TryGetValue(location, out handler))
            {
                ResourcePackage package = string.IsNullOrEmpty(packageName) ? DefaultPackage : YooAssets.GetPackage(packageName);
                if (package == null)
                {
                    return null;
                }

                handler = package.LoadAssetAsync<T>(location);
                await handler.Task;
                resourcesLoader.handlers.Add(location, handler);
            }

            return (T)((AssetHandle)handler).AssetObject;
        }

        public async ETTask<UnityEngine.Object> LoadAssetAsync(string location, Type assetType, CancellationToken cancellationToken = default, string packageName = "")
        {
            if (string.IsNullOrEmpty(location))
            {
                return null;
            }

            using CoroutineLock coroutineLock = await resourcesLoader.Root().GetComponent<CoroutineLockComponent>().Wait(CoroutineLockType.ResourcesLoader, location.GetHashCode());

            HandleBase handler;
            if (!resourcesLoader.handlers.TryGetValue(location, out handler))
            {
                ResourcePackage package = string.IsNullOrEmpty(packageName) ? DefaultPackage : YooAssets.GetPackage(packageName);
                if (package == null)
                {
                    return null;
                }

                handler = package.LoadAssetAsync(location, assetType);
                await handler.Task;
                resourcesLoader.handlers.Add(location, handler);
            }

            return ((AssetHandle)handler).AssetObject;
        }

        public async ETTask<GameObject> LoadGameObjectAsync(string location, Transform parent = null, CancellationToken cancellationToken = default, string packageName = "")
        {
            if (string.IsNullOrEmpty(location))
            {
                return null;
            }

            using CoroutineLock coroutineLock = await resourcesLoader.Root().GetComponent<CoroutineLockComponent>().Wait(CoroutineLockType.ResourcesLoader, location.GetHashCode());

            HandleBase handler;
            if (!resourcesLoader.handlers.TryGetValue(location, out handler))
            {
                ResourcePackage package = string.IsNullOrEmpty(packageName) ? DefaultPackage : YooAssets.GetPackage(packageName);
                if (package == null)
                {
                    return null;
                }

                handler = package.LoadAssetAsync<GameObject>(location);
                await handler.Task;
                resourcesLoader.handlers.Add(location, handler);
            }

            GameObject prefab = ((AssetHandle)handler).AssetObject as GameObject;
            if (prefab == null)
            {
                return null;
            }

            GameObject instance = UnityEngine.Object.Instantiate(prefab, parent);
            return instance;
        }

        public T LoadAsset<T>(string location, string packageName = "") where T : UnityEngine.Object
        {
            if (string.IsNullOrEmpty(location))
            {
                return null;
            }

            lock (resourcesLoader.handlers)
            {
                HandleBase handler;
                if (!resourcesLoader.handlers.TryGetValue(location, out handler))
                {
                    ResourcePackage package = string.IsNullOrEmpty(packageName) ? DefaultPackage : YooAssets.GetPackage(packageName);
                    if (package == null)
                    {
                        return null;
                    }

                    handler = package.LoadAssetSync<T>(location);
                    resourcesLoader.handlers.Add(location, handler);
                }

                return (T)((AssetHandle)handler).AssetObject;
            }
        }

        public UnityEngine.Object LoadAsset(string location, Type assetType, string packageName = "")
        {
            if (string.IsNullOrEmpty(location))
            {
                return null;
            }

            lock (resourcesLoader.handlers)
            {
                HandleBase handler;
                if (!resourcesLoader.handlers.TryGetValue(location, out handler))
                {
                    ResourcePackage package = string.IsNullOrEmpty(packageName) ? DefaultPackage : YooAssets.GetPackage(packageName);
                    if (package == null)
                    {
                        return null;
                    }

                    handler = package.LoadAssetSync(location, assetType);
                    resourcesLoader.handlers.Add(location, handler);
                }

                return ((AssetHandle)handler).AssetObject;
            }
        }

        public GameObject LoadGameObject(string location, Transform parent = null, string packageName = "")
        {
            if (string.IsNullOrEmpty(location))
            {
                return null;
            }

            lock (resourcesLoader.handlers)
            {
                HandleBase handler;
                if (!resourcesLoader.handlers.TryGetValue(location, out handler))
                {
                    ResourcePackage package = string.IsNullOrEmpty(packageName) ? DefaultPackage : YooAssets.GetPackage(packageName);
                    if (package == null)
                    {
                        return null;
                    }

                    handler = package.LoadAssetSync<GameObject>(location);
                    resourcesLoader.handlers.Add(location, handler);
                }

                GameObject prefab = ((AssetHandle)handler).AssetObject as GameObject;
                if (prefab == null)
                {
                    return null;
                }

                GameObject instance = UnityEngine.Object.Instantiate(prefab, parent);
                return instance;
            }
        }

        public async ETTask<Dictionary<string, T>> LoadAllAssetsAsync<T>(string location, CancellationToken cancellationToken = default, string packageName = "") where T : UnityEngine.Object
        {
            if (string.IsNullOrEmpty(location))
            {
                return new Dictionary<string, T>();
            }

            using CoroutineLock coroutineLock = await resourcesLoader.Root().GetComponent<CoroutineLockComponent>().Wait(CoroutineLockType.ResourcesLoader, location.GetHashCode());

            HandleBase handler;
            if (!resourcesLoader.handlers.TryGetValue(location, out handler))
            {
                ResourcePackage package = string.IsNullOrEmpty(packageName) ? DefaultPackage : YooAssets.GetPackage(packageName);
                if (package == null)
                {
                    return new Dictionary<string, T>();
                }

                handler = package.LoadAllAssetsAsync<T>(location);
                await handler.Task;
                resourcesLoader.handlers.Add(location, handler);
            }

            Dictionary<string, T> dictionary = new Dictionary<string, T>();
            foreach (UnityEngine.Object assetObj in ((AllAssetsHandle)handler).AllAssetObjects)
            {
                T t = assetObj as T;
                if (t != null)
                {
                    dictionary.Add(t.name, t);
                }
            }

            return dictionary;
        }

        public async ETTask LoadSceneAsync(string location, LoadSceneMode loadSceneMode, CancellationToken cancellationToken = default, string packageName = "")
        {
            if (string.IsNullOrEmpty(location))
            {
                return;
            }

            using CoroutineLock coroutineLock = await resourcesLoader.Root().GetComponent<CoroutineLockComponent>().Wait(CoroutineLockType.ResourcesLoader, location.GetHashCode());

            HandleBase handler;
            if (resourcesLoader.handlers.TryGetValue(location, out handler))
            {
                return;
            }

            ResourcePackage package = string.IsNullOrEmpty(packageName) ? DefaultPackage : YooAssets.GetPackage(packageName);
            if (package == null)
            {
                return;
            }

            handler = package.LoadSceneAsync(location);
            await handler.Task;
            resourcesLoader.handlers.Add(location, handler);
        }

        public void UnloadAsset(object asset)
        {
            if (asset == null || resourcesLoader == null)
            {
                return;
            }

            // 查找对应的 handler 并释放
            string locationToRemove = null;
            foreach (var kv in resourcesLoader.handlers)
            {
                HandleBase handler = kv.Value;
                if (handler is AssetHandle assetHandle && assetHandle.AssetObject == asset)
                {
                    locationToRemove = kv.Key;
                    assetHandle.Release();
                    break;
                }
            }

            if (locationToRemove != null)
            {
                resourcesLoader.handlers.Remove(locationToRemove);
            }
        }

        public void UnloadUnusedAssets()
        {
            // 资源卸载由 ResourcesLoaderComponent 的 Destroy 方法统一处理
        }

        public void ForceUnloadAllAssets()
        {
            if (resourcesLoader == null)
            {
                return;
            }

            foreach (var kv in resourcesLoader.handlers)
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
            resourcesLoader.handlers.Clear();
        }

        public void OnLowMemory()
        {
            UnloadUnusedAssets();
        }

        public void SetForceUnloadUnusedAssetsAction(Action<bool> action)
        {
            // 可以存储这个 action，在需要时调用
        }
    }

    [EnableClass]
    internal class ResourceLoggerET : YooAsset.ILogger
    {
        public void Log(string message)
        {
            TEngine.Log.Info(message);
        }

        public void Warning(string message)
        {
            TEngine.Log.Warning(message);
        }

        public void Error(string message)
        {
            TEngine.Log.Error(message);
        }

        public void Exception(System.Exception exception)
        {
            TEngine.Log.Fatal(exception.Message);
        }
    }
}
