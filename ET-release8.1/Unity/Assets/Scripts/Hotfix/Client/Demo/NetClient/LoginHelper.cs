namespace ET.Client
{
    public static class LoginHelper
    {
        //
        public static async ETTask Login(Scene root, string account, string password)
        {
            //root是客户端 main fiber 
            root.RemoveComponent<ClientSenderComponent>();//移除链接gate的组建 相当于重新链接
            
            ClientSenderComponent clientSenderComponent = root.AddComponent<ClientSenderComponent>();
            //请求服务器 或者gate服务器分配的一个映射player实体id
            var response = await clientSenderComponent.LoginAsync(account, password);
            if (response.Error!=ErrorCode.ERR_Success)
            {
                Log.Info($"登录失败{response.Error}");
                return;
            }

            root.GetComponent<PlayerComponent>().MyId = response.PlayerId;
            //登录完成
            await EventSystem.Instance.PublishAsync(root, new LoginFinish());
        }
    }
}