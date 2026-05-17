using GameLogic;
using UnityEngine;

namespace ET.Client
{
    [Event(SceneType.Current)]
    public class AfterUnitCreate_CreateUnitView: AEvent<Scene, AfterUnitCreate>
    {
        protected override async ETTask Run(Scene scene, AfterUnitCreate args)
        {
            Unit unit = args.Unit;
            string name = string.IsNullOrEmpty(unit.Name) ? "Empty" : unit.Name;
            // Unit View层
            string assetsName = $"Assets/Bundles/Unit/Unit.prefab";
            GameObject bundleGameObject = await scene.GetComponent<ResourcesLoaderComponent>().LoadAssetAsync<GameObject>(assetsName);
            string prefabKey = "Skeleton"+unit.ConfigId;
            GameObject prefab =bundleGameObject.Get<GameObject>(prefabKey);
            GlobalComponent globalComponent = scene.Root().GetComponent<GlobalComponent>();
            GameObject go = UnityEngine.Object.Instantiate(prefab, globalComponent.Unit, true);
            NetworkCacheComponent netCache = scene.Root().GetComponent<NetworkCacheComponent>();
            bool isMainPlayerUnit = netCache != null && netCache.LoginGamePlayerId != 0 && unit.Id == netCache.LoginGamePlayerId;
            go.name = isMainPlayerUnit ? $"unit_{unit.Id}_{name}*" : $"unit_{unit.Id}_{name}";
            go.transform.position = unit.Position;
            unit.AddComponent<GameObjectComponent>().GameObject = go;
            unit.AddComponent<AnimatorComponent>();
            //Avatar2DComponent avatar2D = unit.AddComponent<Avatar2DComponent,GameObject>(go);
            
            if (isMainPlayerUnit==true&&CameraTestCtrl.Instance!=null)
            {
                CameraTestCtrl.Instance.SetUnit(go.transform);
            }

			//await avatar2D.InitPartsFromBaseAvatarAsync(unit, unit.BaseAvatar);
            
            await ETTask.CompletedTask;
        }
    }
}