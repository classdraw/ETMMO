using UnityEngine;

namespace ET.Client
{
    [EntitySystemOf(typeof(LSUnitViewComponent))]
    public static partial class LSUnitViewComponentSystem
    {
        [EntitySystem]
        private static void Awake(this LSUnitViewComponent self)
        {

        }
        
        [EntitySystem]
        private static void Destroy(this LSUnitViewComponent self)
        {

        }

        public static async ETTask InitAsync(this LSUnitViewComponent self)
        {
            Room room = self.Room();
            LSUnitComponent lsUnitComponent = room.LSWorld.GetComponent<LSUnitComponent>();
            Scene root = self.Root();
            foreach (long playerId in room.PlayerIds)
            {
                LSUnit lsUnit = lsUnitComponent.GetChild<LSUnit>(playerId);
                string assetsName = $"Assets/Bundles/Unit/Unit.prefab";
                GameObject bundleGameObject = await room.GetComponent<ResourcesLoaderComponent>().LoadAssetAsync<GameObject>(assetsName);
                GameObject prefab = bundleGameObject.Get<GameObject>("Skeleton");

                GlobalComponent globalComponent = root.GetComponent<GlobalComponent>();
                GameObject unitGo = UnityEngine.Object.Instantiate(prefab, globalComponent.Unit, true);
                NetworkCacheComponent netCache = root.GetComponent<NetworkCacheComponent>();
                bool isMainPlayerUnit = netCache != null && netCache.LoginGamePlayerId != 0 && lsUnit.Id == netCache.LoginGamePlayerId;
                unitGo.name = isMainPlayerUnit ? $"unit_{lsUnit.Id}*" : $"unit_{lsUnit.Id}";
                unitGo.transform.position = lsUnit.Position.ToVector();

                LSUnitView lsUnitView = self.AddChildWithId<LSUnitView, GameObject>(lsUnit.Id, unitGo);
                lsUnitView.AddComponent<LSAnimatorComponent>();
            }
        }
    }
}