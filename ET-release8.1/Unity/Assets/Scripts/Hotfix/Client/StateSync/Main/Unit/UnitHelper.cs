namespace ET.Client
{
    public static partial class UnitHelper
    {
        public static Unit GetMyUnitFromClientScene(Scene root)
        {
            PlayerComponent playerComponent = root.GetComponent<PlayerComponent>();
            if (playerComponent == null)
            {
                return null;
            }

            CurrentScenesComponent currentScenesComponent = root.GetComponent<CurrentScenesComponent>();
            Scene currentScene = currentScenesComponent?.Scene;
            if (currentScene == null)
            {
                return null;
            }

            UnitComponent unitComponent = currentScene.GetComponent<UnitComponent>();
            if (unitComponent == null)
            {
                return null;
            }

            return unitComponent.Get(playerComponent.MyId);
        }
        
        public static Unit GetMyUnitFromCurrentScene(Scene currentScene)
        {
            PlayerComponent playerComponent = currentScene.Root().GetComponent<PlayerComponent>();
            return currentScene.GetComponent<UnitComponent>().Get(playerComponent.MyId);
        }
    }
}