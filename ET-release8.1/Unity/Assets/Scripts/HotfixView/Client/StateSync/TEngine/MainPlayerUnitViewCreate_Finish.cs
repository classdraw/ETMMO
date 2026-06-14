namespace ET.Client
{
    /// <summary>
    /// 主角创建成功后处理逻辑
    /// </summary>
    [Event(SceneType.StateSync)]
    public class MainPlayerUnitViewCreate_Finish : AEvent<Scene, MainPlayerUnitViewCreate>
    {
        protected override async ETTask Run(Scene root, MainPlayerUnitViewCreate args)
        {
            Unit unit = args.Unit;
            GameObjectComponent gameObjectComponent = unit?.GetComponent<GameObjectComponent>();
            if (gameObjectComponent == null)
            {
                return;
            }
            //可能还要干别的
            //相机绑定
            root.GetComponent<CameraPlayComponent>()?.BindPlayer(gameObjectComponent.GameObject);
            await ETTask.CompletedTask;
        }
    }
}
