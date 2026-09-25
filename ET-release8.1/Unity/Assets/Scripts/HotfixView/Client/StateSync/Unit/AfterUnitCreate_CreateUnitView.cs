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
                case UnitType.Bullet:
                {
                    await CreateBullet(scene, args);
                    break;
                }
            }

            await ETTask.CompletedTask;
        }

        private async ETTask CreateBullet(Scene scene, AfterUnitCreate args)
        {
            Unit unit = args.Unit;
            if (!BulletConfigCategory.Instance.Contain(unit.TableConfigId))
            {
                Log.Error($"BulletConfig not found: tableConfigId={unit.TableConfigId}, unitConfigId={unit.ConfigId}");
                return;
            }

            BulletConfig bulletConfig = BulletConfigCategory.Instance.Get(unit.TableConfigId);
            if (string.IsNullOrEmpty(bulletConfig.Model))
            {
                Log.Error($"BulletConfig model empty: tableConfigId={unit.TableConfigId}");
                return;
            }

            string prefabAssetPath = $"Assets/Bundles/Bullet/{bulletConfig.Model}.prefab";
            ResourcesLoaderComponent loader = scene.GetComponent<ResourcesLoaderComponent>();
            GameObject prefab = await loader.LoadAssetAsync<GameObject>(prefabAssetPath);
            if (prefab == null)
            {
                Log.Error($"Bullet prefab not found: {prefabAssetPath}");
                return;
            }

            GlobalComponent globalComponent = scene.Root().GetComponent<GlobalComponent>();
            GameObject go = UnityEngine.Object.Instantiate(prefab, globalComponent.Unit, true);
            go.name = $"bullet_{unit.Id}_{bulletConfig.Model}";
            go.transform.position = unit.Position;
            go.transform.rotation = unit.Rotation;
            unit.AddComponent<GameObjectComponent>().GameObject = go;
            await ETTask.CompletedTask;
        }

        private async ETTask CreateMonster(Scene scene, AfterUnitCreate args)
        {
            Unit unit = args.Unit;
            if (!MonsterConfigCategory.Instance.Contain(unit.TableConfigId))
            {
                Log.Error($"MonsterConfig not found: tableConfigId={unit.TableConfigId}, unitConfigId={unit.ConfigId}");
                return;
            }

            MonsterConfig monsterConfig = MonsterConfigCategory.Instance.Get(unit.TableConfigId);
            if (string.IsNullOrEmpty(monsterConfig.Model))
            {
                Log.Error($"MonsterConfig model empty: tableConfigId={unit.TableConfigId}");
                return;
            }

            if (!TryPrepareMonsterModel(unit, monsterConfig, out string prefabAssetPath))
            {
                return;
            }

            ResourcesLoaderComponent loader = scene.GetComponent<ResourcesLoaderComponent>();
            GameObject prefab = await loader.LoadAssetAsync<GameObject>(prefabAssetPath);
            if (prefab == null)
            {
                Log.Error($"Monster prefab not found: {prefabAssetPath}");
                return;
            }

            string displayName = string.IsNullOrEmpty(unit.Name) ? monsterConfig.Model : unit.Name;
            GlobalComponent globalComponent = scene.Root().GetComponent<GlobalComponent>();
            GameObject go = UnityEngine.Object.Instantiate(prefab, globalComponent.Unit, true);
            go.name = $"monster_{unit.Id}_{displayName}";
            go.transform.position = unit.Position;
            go.transform.rotation = Quaternion.identity;
            AttachMonsterViewComponents(unit, go);
            await ETTask.CompletedTask;
        }

        /// <summary>
        /// ModelType=0：Monster.prefab 骨架 + Model 部位拼装。
        /// ModelType=1：Model 指定整模 prefab，脚本栈相同，预制体已做好贴图无需拼装。
        /// </summary>
        private static bool TryPrepareMonsterModel(Unit unit, MonsterConfig monsterConfig, out string prefabAssetPath)
        {
            prefabAssetPath = null;
            switch (monsterConfig.ModelType)
            {
                case (int)MonsterModelType.PartAssembly:
                {
                    if (!ExternalDisplayHelper.TryParseExternalDisplayString(monsterConfig.Model, out _))
                    {
                        Log.Error($"Monster part assembly Model invalid: monsterConfigId={monsterConfig.Id}, model={monsterConfig.Model}");
                        return false;
                    }

                    if (string.IsNullOrEmpty(unit.BaseExternalDisplay))
                    {
                        unit.BaseExternalDisplay = monsterConfig.Model;
                    }

                    prefabAssetPath = "Assets/Bundles/Unit/Monster.prefab";
                    return true;
                }
                case (int)MonsterModelType.Prefab:
                {
                    prefabAssetPath = $"Assets/Bundles/Unit/{monsterConfig.Model}.prefab";
                    return true;
                }
                default:
                {
                    Log.Error($"Unknown Monster ModelType: {monsterConfig.ModelType}, monsterConfigId={monsterConfig.Id}");
                    return false;
                }
            }
        }

        /// <summary>
        /// 两种 ModelType 共用同一套 2D View 组件；仅 PartAssembly 会通过 Avatar2D 拼装部件。
        /// </summary>
        private static void AttachMonsterViewComponents(Unit unit, GameObject go)
        {
            unit.AddComponent<GameObjectComponent>().GameObject = go;
            unit.AddComponent<Avatar2DComponent>();
            unit.AddComponent<Animator2DComponent>();
            unit.AddComponent<MountComponent>();
            unit.AddComponent<UnitTopUIComponent>();
        }

        private async ETTask CreatePlayer(Scene scene, AfterUnitCreate args)
        {
            Unit unit = args.Unit;
            Log.Info($"AfterUnitCreate_CreateUnitView Player unitId={unit.Id}, name={unit.Name}, configId={unit.ConfigId}, race={unit.Race}, gender={unit.Gender}, baseExternalDisplay={unit.BaseExternalDisplay ?? string.Empty}");
            string name = string.IsNullOrEmpty(unit.Name) ? "Empty" : unit.Name;
            // Unit View层
            string assetsName = $"Assets/Bundles/Unit/Unit.prefab";
            GameObject bundleGameObject = await scene.GetComponent<ResourcesLoaderComponent>().LoadAssetAsync<GameObject>(assetsName);
            string prefabKey = "Skeleton1001";
            GameObject prefab = bundleGameObject.Get<GameObject>(prefabKey);
            GlobalComponent globalComponent = scene.Root().GetComponent<GlobalComponent>();
            GameObject go = UnityEngine.Object.Instantiate(prefab, globalComponent.Unit, true);
            NetworkCacheComponent netCache = scene.Root().GetComponent<NetworkCacheComponent>();
            bool isMainPlayerUnit = netCache != null && netCache.LoginGamePlayerId != 0 && unit.Id == netCache.LoginGamePlayerId;
            go.name = isMainPlayerUnit ? $"unit_{unit.Id}_{name}*" : $"unit_{unit.Id}_{name}";
            go.transform.position = unit.Position;
            go.transform.rotation = Quaternion.identity;
            unit.AddComponent<GameObjectComponent>().GameObject = go;
            unit.AddComponent<Avatar2DComponent>();
            unit.AddComponent<Animator2DComponent>();
            unit.AddComponent<MountComponent>();
            unit.AddComponent<UnitTopUIComponent>();

            if (isMainPlayerUnit)
            {
                EventSystem.Instance.Publish(scene.Root(), new MainPlayerUnitViewCreate { Unit = unit });
            }
            await ETTask.CompletedTask;
        }
    }
}