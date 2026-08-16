using UnityEngine;

namespace ET.Client
{
    [EntitySystemOf(typeof(OperaComponent))]
    [FriendOf(typeof(OperaComponent))]
    [FriendOf(typeof(CameraPlayComponent))]
    public static partial class OperaComponentSystem
    {
        [EntitySystem]
        private static void Awake(this OperaComponent self)
        {
            self.mapMask = LayerMask.GetMask("Ground");
        }

        [EntitySystem]
        private static void Update(this OperaComponent self)
        {
            if (Input.GetMouseButtonDown(1))
            {
                if (!self.TryGetClickWorldPosition(out Vector3 clickPosition))
                {
                    return;
                }

                Unit myUnit = UnitHelper.GetMyUnitFromCurrentScene(self.Root().CurrentScene());
                if (myUnit == null || myUnit.IsDisposed || myUnit.IsCasting())
                {
                    return;
                }

                C2M_PathfindingResult c2MPathfindingResult = C2M_PathfindingResult.Create();
                c2MPathfindingResult.Position = clickPosition;
                self.Root().GetComponent<ClientSenderComponent>().Send(c2MPathfindingResult);
            }
            
            if (Input.GetKeyDown(KeyCode.Q))
            {
                self.Test1().Coroutine();
            }
                
            if (Input.GetKeyDown(KeyCode.W))
            {
                self.Test2().Coroutine();
            }

            if (Input.GetKeyDown(KeyCode.R))
            {
                CodeLoader.Instance.Reload();
                return;
            }

            if (Input.GetKeyDown(KeyCode.T))
            {
                C2M_TransferMap c2MTransferMap = C2M_TransferMap.Create();
                c2MTransferMap.MapConfigId = 10002;
                c2MTransferMap.MapFiberId = 0;//去指定分区地图用到
                
                self.Root().GetComponent<ClientSenderComponent>().Call(c2MTransferMap).Coroutine();
            }
            if (Input.GetKeyDown(KeyCode.M))
            {
                C2M_TransferMap c2MTransferMap = C2M_TransferMap.Create();
                c2MTransferMap.MapConfigId = 10001;
                c2MTransferMap.MapFiberId = 0;//去指定分区地图用到
                self.Root().GetComponent<ClientSenderComponent>().Call(c2MTransferMap).Coroutine();
            }
        }
        
        private static bool TryGetClickWorldPosition(this OperaComponent self, out Vector3 clickPosition)
        {
            clickPosition = Vector3.zero;
            Camera camera = self.GetPlayCamera();
            if (camera == null)
            {
                return false;
            }

            Ray ray = camera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, 1000, self.mapMask))
            {
                clickPosition = hit.point;
                return true;
            }

            Unit myUnit = UnitHelper.GetMyUnitFromCurrentScene(self.Root().CurrentScene());
            float groundY = myUnit != null ? myUnit.Position.y : 0f;
            Plane xzPlane = new Plane(Vector3.up, new Vector3(0f, groundY, 0f));
            if (!xzPlane.Raycast(ray, out float enter))
            {
                return false;
            }

            clickPosition = ray.GetPoint(enter);
            return true;
        }

        private static Camera GetPlayCamera(this OperaComponent self)
        {
            CameraPlayComponent cameraPlay = self.Root().GetComponent<CameraPlayComponent>();
            if (cameraPlay != null && cameraPlay.MainCameraObj != null)
            {
                Camera playCamera = cameraPlay.MainCameraObj.GetComponent<Camera>();
                if (playCamera != null)
                {
                    return playCamera;
                }
            }

            return Camera.main;
        }

        private static async ETTask Test1(this OperaComponent self)
        {
            await BattleHelper.GMCastSimple(self.Root(), 66001);
        }
            
        private static async ETTask Test2(this OperaComponent self)
        {
            await BattleHelper.GMCastSimple(self.Root(), 66002);
        }
    }
}