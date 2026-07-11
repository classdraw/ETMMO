namespace ET.Client
{
    public static partial class SceneChangeHelper
    {
        // 场景切换协程
        public static async ETTask SceneChangeTo(Scene root, string sceneName, long sceneInstanceId)
        {
            root.RemoveComponent<AIComponent>();
            
            CurrentScenesComponent currentScenesComponent = root.GetComponent<CurrentScenesComponent>();
            currentScenesComponent.Scene?.Dispose(); // 删除之前的CurrentScene，创建新的
            Scene currentScene = CurrentSceneFactory.Create(sceneInstanceId, sceneName, currentScenesComponent);
            UnitComponent unitComponent = currentScene.AddComponent<UnitComponent>();
         
            // 可以订阅这个事件中创建Loading界面
            EventSystem.Instance.Publish(root, new SceneChangeStart());
            // 等待CreateMyUnit的消息
            Wait_CreateMyUnit waitCreateMyUnit = await root.GetComponent<ObjectWait>().Wait<Wait_CreateMyUnit>();
            M2C_CreateMyUnit m2CCreateMyUnit = waitCreateMyUnit.Message;
            Unit unit = UnitFactory.Create(currentScene, m2CCreateMyUnit.Unit);
            unitComponent.Add(unit);
            root.GetComponent<PlayerComponent>().MyId = unit.Id;
            root.RemoveComponent<AIComponent>();
            
            EventSystem.Instance.Publish(currentScene, new SceneChangeFinish());//下一个场景要干的事
            
            EventSystem.Instance.Publish(root, new SceneChangeFinish());//root场景订阅 那么自己销毁
            // 通知等待场景切换的协程
            root.GetComponent<ObjectWait>().Notify(new Wait_SceneChangeFinish());
        }

        /// <summary>
        /// 客户端主动切场景：不等待 M2C_CreateMyUnit，仅走 SceneChangeStart / LoadScene 与 SceneChangeFinish。
        /// </summary>
        public static async ETTask SceneChangeToSimple(Scene root, string sceneName, long sceneInstanceId)
        {
            root.RemoveComponent<AIComponent>();

            CurrentScenesComponent currentScenesComponent = root.GetComponent<CurrentScenesComponent>();
            currentScenesComponent.Scene?.Dispose();
            Scene currentScene = CurrentSceneFactory.Create(sceneInstanceId, sceneName, currentScenesComponent);
            currentScene.AddComponent<UnitComponent>();

            EventSystem.Instance.Publish(root, new SceneChangeStart());//MAIN

            root.RemoveComponent<AIComponent>();

            EventSystem.Instance.Publish(currentScene, new SceneChangeFinish());
            EventSystem.Instance.Publish(root, new SceneChangeFinish());//MAIN
            root.GetComponent<ObjectWait>().Notify(new Wait_SceneChangeFinish());
            await ETTask.CompletedTask;
        }
    }
}
