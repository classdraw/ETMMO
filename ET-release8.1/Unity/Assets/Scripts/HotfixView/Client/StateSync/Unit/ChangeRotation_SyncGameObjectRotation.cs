using UnityEngine;

namespace ET.Client
{
    [Event(SceneType.Current)]
    public class ChangeRotation_SyncGameObjectRotation: AEvent<Scene, ChangeRotation>
    {
        protected override async ETTask Run(Scene scene, ChangeRotation args)
        {
            Unit unit = args.Unit;
            GameObjectComponent gameObjectComponent = unit.GetComponent<GameObjectComponent>();
            if (gameObjectComponent == null)
            {
                return;
            }

            if (ShouldSkipGameObjectRotation(unit))
            {
                await ETTask.CompletedTask;
                return;
            }

            Transform transform = gameObjectComponent.GameObject.transform;
            transform.rotation = unit.Rotation;
            await ETTask.CompletedTask;
        }

        private static bool ShouldSkipGameObjectRotation(Unit unit)
        {
            switch (unit.Type())
            {
                case UnitType.Player:
                case UnitType.Monster:
                case UnitType.NPC:
                case UnitType.Pet:
                case UnitType.Summon:
                case UnitType.Robot:
                    return true;
                default:
                    return false;
            }
        }
    }
}
