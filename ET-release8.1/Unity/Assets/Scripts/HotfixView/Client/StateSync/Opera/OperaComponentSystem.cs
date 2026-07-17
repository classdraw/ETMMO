using UnityEngine;

namespace ET.Client
{
    [EntitySystemOf(typeof(OperaComponent))]
    [FriendOf(typeof(OperaComponent))]
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
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit, 1000, self.mapMask))
                {
                    Unit myUnit = UnitHelper.GetMyUnitFromCurrentScene(self.Root().CurrentScene());
                    if (myUnit == null || myUnit.IsDisposed || myUnit.IsCasting())
                    {
                        return;
                    }

                    C2M_PathfindingResult c2MPathfindingResult = C2M_PathfindingResult.Create();
                    c2MPathfindingResult.Position = hit.point;
                    self.Root().GetComponent<ClientSenderComponent>().Send(c2MPathfindingResult);
                }
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
        
        private static async ETTask Test1(this OperaComponent self)
        {
            await BattleHelper.GMCastSimple(self.Root(), 66001);
        }
            
        private static async ETTask Test2(this OperaComponent self)
        {
            Log.Debug($"Croutine 2 start2");
            using (await self.Root().GetComponent<CoroutineLockComponent>().Wait(1, 20000, 3000))
            {
                await self.Root().GetComponent<TimerComponent>().WaitAsync(1000);
            }
            Log.Debug($"Croutine 2 end2");
        }
    }
}