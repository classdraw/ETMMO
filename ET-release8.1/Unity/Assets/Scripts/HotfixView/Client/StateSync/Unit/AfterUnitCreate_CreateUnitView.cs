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
            switch (unit.Type())
            {
                case UnitType.Player:
                {
                    await CreatePlayer(scene, args);
                    break;
                }
                case UnitType.Monster:
                {
                    await CreateMonster(scene, args);
                    break;
                }
            }

            await ETTask.CompletedTask;
        }

        private static MonsterConfig GetMonsterConfigByUnitConfigId(int unitConfigId)
        {
            foreach (MonsterConfig monsterConfig in MonsterConfigCategory.Instance.GetAll().Values)
            {
                if (monsterConfig.UnitConfigId == unitConfigId)
                {
                    return monsterConfig;
                }
            }

            return null;
        }

        private async ETTask CreateMonster(Scene scene, AfterUnitCreate args)
        {
            Unit unit = args.Unit;
            MonsterConfig monsterConfig = GetMonsterConfigByUnitConfigId(unit.ConfigId);
            if (monsterConfig == null || string.IsNullOrEmpty(monsterConfig.Model))
            {
                Log.Error($"MonsterConfig not found: unitConfigId={unit.ConfigId}");
                return;
            }

            string displayName = string.IsNullOrEmpty(unit.Name) ? monsterConfig.Model : unit.Name;
            string assetsName = $"Assets/Bundles/Unit/{monsterConfig.Model}.prefab";
            GameObject prefab = await scene.GetComponent<ResourcesLoaderComponent>().LoadAssetAsync<GameObject>(assetsName);
            if (prefab == null)
            {
                Log.Error($"Monster prefab not found: {assetsName}");
                return;
            }

            GlobalComponent globalComponent = scene.Root().GetComponent<GlobalComponent>();
            GameObject go = UnityEngine.Object.Instantiate(prefab, globalComponent.Unit, true);
            go.name = $"monster_{unit.Id}_{displayName}";
            go.transform.position = unit.Position;
            unit.AddComponent<GameObjectComponent>().GameObject = go;
            unit.AddComponent<AnimatorComponent>();
            unit.AddComponent<UnitTopUIComponent>();
            await ETTask.CompletedTask;
        }

        private async ETTask CreatePlayer(Scene scene, AfterUnitCreate args)
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
            unit.AddComponent<UnitTopUIComponent>();
            //Avatar2DComponent avatar2D = unit.AddComponent<Avatar2DComponent,GameObject>(go);
            
            if (isMainPlayerUnit)
            {
                EventSystem.Instance.Publish(scene.Root(), new MainPlayerUnitViewCreate { Unit = unit });
            }
            await ETTask.CompletedTask;
        }
    }
}