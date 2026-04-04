namespace ET.Client
{
    public static class UIHelper
    {
        [EnableAccessEntiyChild]
        public static async ETTask<UI> Create(Entity scene, string uiType)
        {
            return await scene.GetComponent<UIComponent>().Create(uiType);

        }
        
        [EnableAccessEntiyChild]
        public static async ETTask Remove(Entity scene, string uiType)
        {
            scene.GetComponent<UIComponent>().Remove(uiType);
            await ETTask.CompletedTask;
        }
    }
}