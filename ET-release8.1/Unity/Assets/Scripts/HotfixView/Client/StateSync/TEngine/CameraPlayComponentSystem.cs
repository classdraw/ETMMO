using Cinemachine;
using UnityEngine;

namespace ET.Client
{
    [FriendOf(typeof(CameraPlayComponent))]
    [EntitySystemOf(typeof(CameraPlayComponent))]
    public static partial class CameraPlayComponentSystem
    {
        
        [EntitySystem]
        private static void Awake(this CameraPlayComponent self)
        {

        }
        
        [EntitySystem]
        private static void Destroy(this CameraPlayComponent self)
        {
            if (self.CameraRootObj!=null)
            {
                GameObject.Destroy(self.CameraRootObj);
            }

            self.CameraRootObj = null;
            self.CinemachineFreeLook = null;
        }

        public static async ETTask Init(this CameraPlayComponent self)
        {
            var resLoader = self.Root().GetComponent<ResourcesLoaderComponent>();
            GameObject cameraRoot = await LoadGameObjectInstance(resLoader, self.CameraRootPath);
            GameObject.DontDestroyOnLoad(cameraRoot);
            cameraRoot.transform.position = Vector3.zero;
            cameraRoot.name = "CameraPlayComponent(Object)";
            self.CameraRootObj = cameraRoot;
            self.CinemachineFreeLook = cameraRoot.transform.Find("CameraFree").GetComponent<CinemachineFreeLook>();
            
            await ETTask.CompletedTask;
        }

        public static void BindPlayer(this CameraPlayComponent self, GameObject playerObject)
        {
            if (playerObject == null)
            {
                return;
            }

            self.PlayerObject = playerObject;
            if (self.CinemachineFreeLook == null)
            {
                return;
            }

            Transform target = playerObject.transform;
            self.CinemachineFreeLook.Follow = target;
            self.CinemachineFreeLook.LookAt = target;
        }
        
        private static async ETTask<GameObject> LoadGameObjectInstance(ResourcesLoaderComponent resLoader,string location)
        {
            var bundleGameObject = await resLoader.LoadAssetAsync<GameObject>(location);
            GameObject gameObject = UnityEngine.Object.Instantiate(bundleGameObject) as GameObject;
            return gameObject;

        }
    }
}

