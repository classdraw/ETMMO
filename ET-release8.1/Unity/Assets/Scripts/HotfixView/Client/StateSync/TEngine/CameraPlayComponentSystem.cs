using Cinemachine;
using UnityEngine;

namespace ET.Client
{
    [FriendOf(typeof(CameraPlayComponent))]
    [EntitySystemOf(typeof(CameraPlayComponent))]
    [FriendOf(typeof(GlobalComponent))]
    public static partial class CameraPlayComponentSystem
    {
        
        [EntitySystem]
        private static void Awake(this CameraPlayComponent self)
        {

        }
        
        [EntitySystem]
        private static void Destroy(this CameraPlayComponent self)
        {
            if (self.CameraRootObj != null)
            {
                GameObject.Destroy(self.CameraRootObj);
            }

            if (self.MainCameraObj != null)
            {
                GameObject.Destroy(self.MainCameraObj);
            }

            self.CameraRootObj = null;
            self.MainCameraObj = null;
            self.FollowCamera = null;
        }

        public static async ETTask Init(this CameraPlayComponent self)
        {
            ResourcesLoaderComponent resLoader = self.Root().GetComponent<ResourcesLoaderComponent>();
            GlobalComponent globalComponent = self.Root().GetComponent<GlobalComponent>();

            GameObject mainCamera = await LoadGameObjectInstance(resLoader, self.MainCameraPath);
            mainCamera.transform.SetParent(globalComponent.Global, false);
            mainCamera.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.Euler(90f, 0f, 0f));
            mainCamera.name = "CameraPlayComponent(Camera)";
            self.MainCameraObj = mainCamera;

            GameObject cameraRoot = await LoadGameObjectInstance(resLoader, self.CameraRootPath);
            GameObject.DontDestroyOnLoad(cameraRoot);
            cameraRoot.transform.position = Vector3.zero;
            cameraRoot.name = "CameraPlayComponent(CameraRoot)";
            self.CameraRootObj = cameraRoot;
            self.FollowCamera = cameraRoot.GetComponentInChildren<CinemachineVirtualCameraBase>(true);
            if (self.FollowCamera == null)
            {
                Log.Error("CameraPlayComponent Init failed, Cinemachine virtual camera not found under CameraRoot.");
            }

            await ETTask.CompletedTask;
        }

        public static void BindPlayer(this CameraPlayComponent self, GameObject playerObject)
        {
            if (playerObject == null)
            {
                return;
            }

            self.PlayerObject = playerObject;
            if (self.FollowCamera == null)
            {
                return;
            }

            self.FollowCamera.Follow = playerObject.transform;
        }
        
        private static async ETTask<GameObject> LoadGameObjectInstance(ResourcesLoaderComponent resLoader,string location)
        {
            var bundleGameObject = await resLoader.LoadAssetAsync<GameObject>(location);
            GameObject gameObject = UnityEngine.Object.Instantiate(bundleGameObject) as GameObject;
            return gameObject;

        }
    }
}

