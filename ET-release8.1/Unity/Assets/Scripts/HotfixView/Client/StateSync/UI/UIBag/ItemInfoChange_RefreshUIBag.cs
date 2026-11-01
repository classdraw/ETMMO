namespace ET.Client
{
    [Event(SceneType.StateSync)]
    public class ItemInfoChange_RefreshUIBag : AEvent<Scene, ItemInfoChange>
    {
        protected override async ETTask Run(Scene root, ItemInfoChange args)
        {
            if (args.ItemProto.ContainerType != (int)KnapsackContainerType.Inventory)
            {
                await ETTask.CompletedTask;
                return;
            }

            Scene currentScene = root.CurrentScene();
            if (currentScene == null)
            {
                await ETTask.CompletedTask;
                return;
            }

            UIComponent uiComponent = currentScene.GetComponent<UIComponent>();
            if (uiComponent == null)
            {
                await ETTask.CompletedTask;
                return;
            }

            UI bagUi = uiComponent.Get(UIType.UIBag);
            if (bagUi == null)
            {
                await ETTask.CompletedTask;
                return;
            }

            UIBagComponent bag = bagUi.GetComponent<UIBagComponent>();
            bag?.RefreshFromClientKnapsack();
            await ETTask.CompletedTask;
        }
    }
}
