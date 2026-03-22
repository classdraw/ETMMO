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
                Log.Error($"登录失败{response.Error}");
                return;
            }

            string token = response.Token;
            //获取服务器列表
            //C2R_GET
            C2R_GetServerInfos c2RGetServerInfos = C2R_GetServerInfos.Create();
            c2RGetServerInfos.Token = token;
            c2RGetServerInfos.AccountName = account;
            
            R2C_GetServerInfos r2CGetServerInfos = await clientSenderComponent.Call(c2RGetServerInfos) as R2C_GetServerInfos;
            if (r2CGetServerInfos.Error!=ErrorCode.ERR_Success)
            {
                Log.Error("请求服务器列表失败");
                return;
            }

            //Log.Info("   "+r2CGetServerInfos.ServerInfoList.Count);

            ServerInfoProto serverInfoProto = r2CGetServerInfos.ServerInfoList[0];
            //获得区服角色列表
            C2R_GetRoles c2RGetRoles = C2R_GetRoles.Create();
            c2RGetRoles.Token = token;
            c2RGetRoles.AccountName = account;
            c2RGetRoles.ServerId = serverInfoProto.Id;
            R2C_GetRoles r2CGetRoles=await clientSenderComponent.Call(c2RGetRoles) as R2C_GetRoles;
            if (r2CGetRoles.Error!=ErrorCode.ERR_Success)
            {
                Log.Error("请求区服角色列表失败");
                return;
            }
            
            Log.Info(">>>>>>>"+r2CGetRoles.RoleInfoList.Count);


            /*
            root.GetComponent<PlayerComponent>().MyId = response.PlayerId;
            //登录完成
            await EventSystem.Instance.PublishAsync(root, new LoginFinish());*/
        }
    }
}