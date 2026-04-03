namespace ET.Client
{
    public static class UIHelper
    {
        [EnableAccessEntiyChild]
        public static async ETTask<UI> Create(Entity scene, string uiType, UILayer uiLayer)
        {
            //return await scene.GetComponent<UIComponent>().Create(uiType, uiLayer);
            await ETTask.CompletedTask;
            return new UI();
        }
        
        [EnableAccessEntiyChild]
        public static async ETTask Remove(Entity scene, string uiType)
        {
            //scene.GetComponent<UIComponent>().Remove(uiType);
            await ETTask.CompletedTask;
        }
    }
}