using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.SceneManagement;
using YooAsset;
using ET;

namespace TEngine
{
    internal sealed partial class ResourceModuleET : Module, IResourceModuleET
    {
        public IResourceModuleET ResAgent { get; set; }

        public string DefaultPackageName
        {
            get => ResAgent?.DefaultPackageName ?? "DefaultPackage";
            set
            {
                if (ResAgent != null)
                {
                    ResAgent.DefaultPackageName = value;
                }
            }
        }

        public long Milliseconds
        {
            get => ResAgent?.Milliseconds ?? 30;
            set
            {
                if (ResAgent != null)
                {
                    ResAgent.Milliseconds = value;
                }
            }
        }

        public override void OnInit()
        {
        }

        public override void Shutdown()
        {
        }

        public AssetHandle LoadAssetSyncHandle<T>(string location, string packageName = "") where T : UnityEngine.Object
        {
            return ResAgent?.LoadAssetSyncHandle<T>(location, packageName);
        }

        public AssetHandle LoadAssetSyncHandle(string location, Type assetType, string packageName = "")
        {
            return ResAgent?.LoadAssetSyncHandle(location, assetType, packageName);
        }

        public AssetHandle LoadAssetAsyncHandle<T>(string location, string packageName = "") where T : UnityEngine.Object
        {
            return ResAgent?.LoadAssetAsyncHandle<T>(location, packageName);
        }

        public AssetHandle LoadAssetAsyncHandle(string location, Type type, string packageName = "")
        {
            return ResAgent?.LoadAssetAsyncHandle(location, type, packageName);
        }

        public async ETTask LoadAsset<T>(string location, Action<T> callback, string packageName = "") where T : UnityEngine.Object
        {
            if (ResAgent != null)
            {
                await ResAgent.LoadAsset<T>(location, callback, packageName);
            }
        }

        public async ETTask<T> LoadAssetAsync<T>(string location, CancellationToken cancellationToken = default, string packageName = "") where T : UnityEngine.Object
        {
            if (ResAgent != null)
            {
                return await ResAgent.LoadAssetAsync<T>(location, cancellationToken, packageName);
            }
            return null;
        }

        public async ETTask<UnityEngine.Object> LoadAssetAsync(string location, Type assetType, CancellationToken cancellationToken = default, string packageName = "")
        {
            if (ResAgent != null)
            {
                return await ResAgent.LoadAssetAsync(location, assetType, cancellationToken, packageName);
            }
            return null;
        }

        public async ETTask<GameObject> LoadGameObjectAsync(string location, Transform parent = null, CancellationToken cancellationToken = default, string packageName = "")
        {
            if (ResAgent != null)
            {
                return await ResAgent.LoadGameObjectAsync(location, parent, cancellationToken, packageName);
            }
            return null;
        }

        public T LoadAsset<T>(string location, string packageName = "") where T : UnityEngine.Object
        {
            return ResAgent?.LoadAsset<T>(location, packageName);
        }

        public UnityEngine.Object LoadAsset(string location, Type assetType, string packageName = "")
        {
            return ResAgent?.LoadAsset(location, assetType, packageName);
        }

        public GameObject LoadGameObject(string location, Transform parent = null, string packageName = "")
        {
            return ResAgent?.LoadGameObject(location, parent, packageName);
        }

        public async ETTask<Dictionary<string, T>> LoadAllAssetsAsync<T>(string location, CancellationToken cancellationToken = default, string packageName = "") where T : UnityEngine.Object
        {
            if (ResAgent != null)
            {
                return await ResAgent.LoadAllAssetsAsync<T>(location, cancellationToken, packageName);
            }
            return new Dictionary<string, T>();
        }

        public async ETTask LoadSceneAsync(string location, LoadSceneMode loadSceneMode, CancellationToken cancellationToken = default, string packageName = "")
        {
            if (ResAgent != null)
            {
                await ResAgent.LoadSceneAsync(location, loadSceneMode, cancellationToken, packageName);
            }
        }

        public void UnloadAsset(object asset)
        {
            ResAgent?.UnloadAsset(asset);
        }

        public void UnloadUnusedAssets()
        {
            ResAgent?.UnloadUnusedAssets();
        }

        public void ForceUnloadAllAssets()
        {
            ResAgent?.ForceUnloadAllAssets();
        }

        public void OnLowMemory()
        {
            ResAgent?.OnLowMemory();
        }

        public void SetForceUnloadUnusedAssetsAction(Action<bool> action)
        {
            ResAgent?.SetForceUnloadUnusedAssetsAction(action);
        }
    }
}
