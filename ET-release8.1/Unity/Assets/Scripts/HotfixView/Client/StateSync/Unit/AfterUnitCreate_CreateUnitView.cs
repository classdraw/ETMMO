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
            // Unit View层
            string assetsName = $"Assets/Bundles/Unit/Unit.prefab";
            GameObject bundleGameObject = await scene.GetComponent<ResourcesLoaderComponent>().LoadAssetAsync<GameObject>(assetsName);
            GameObject prefab = bundleGameObject.Get<GameObject>("Skeleton");

            GlobalComponent globalComponent = scene.Root().GetComponent<GlobalComponent>();
            GameObject go = UnityEngine.Object.Instantiate(prefab, globalComponent.Unit, true);
            NetworkCacheComponent netCache = scene.Root().GetComponent<NetworkCacheComponent>();
            bool isMainPlayerUnit = netCache != null && netCache.LoginGamePlayerId != 0 && unit.Id == netCache.LoginGamePlayerId;
            go.name = isMainPlayerUnit ? $"unit_{unit.Id}*" : $"unit_{unit.Id}";
            go.transform.position = unit.Position;
            unit.AddComponent<GameObjectComponent>().GameObject = go;
            unit.AddComponent<AnimatorComponent>();
            Avatar2DComponent avatar2D = unit.AddComponent<Avatar2DComponent,GameObject>(go);

            var unitObj=go.Get<GameObject>("Unit");
            unitObj.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            if (isMainPlayerUnit==true&&CameraTestCtrl.Instance!=null)
            {
                CameraTestCtrl.Instance.SetUnit(go.transform);
            }

			await avatar2D.InitPartsFromBaseAvatarAsync(unit, unit.BaseAvatar);
            
            await ETTask.CompletedTask;
        }
        //测试数值同步
        private async ETTask AAA(Scene scene)
        {
            ClientSenderComponent clientSender = scene.Root().GetComponent<ClientSenderComponent>();
            if (clientSender == null)
            {
                return;
            }

            C2M_TestNumericValue request = C2M_TestNumericValue.Create();
            IResponse response = await clientSender.Call(request, false);
            if (response is M2C_TestNumericValue m2C)
            {
                Log.Info($"C2M_TestNumericValue Error={m2C.Error} response={m2C.response}");
            }
        }
    }
}