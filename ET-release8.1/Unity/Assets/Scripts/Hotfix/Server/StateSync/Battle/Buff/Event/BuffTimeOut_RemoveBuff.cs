namespace ET.Server
{
    [Event(SceneType.Map)]
    public class BuffTimeOut_RemoveBuff:AEvent<Scene,BuffTimeOut>
    {
        protected override async ETTask Run(Scene scene,BuffTimeOut timeOut)
        {
            timeOut.Unit.GetComponent<BuffComponent>()?.Remove(timeOut.BuffId);
            
            await ETTask.CompletedTask;
        }
    }
}

