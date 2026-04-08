using CommandLine;

namespace ET.Client
{
    public static class LoginHelper
    {
        //总登录流程 现在拆解
        public static async ETTask LoginOld(Scene root, string account, string password)
        {
            //root是客户端 main fiber 
            root.RemoveComponent<ClientSenderComponent>();//移除链接gate的组建 相当于重新链接
            
            ClientSenderComponent clientSenderComponent = root.AddComponent<ClientSenderComponent>();
            //请求服务器 或者gate服务器分配的一个映射player实体id
            var response = await clientSenderComponent.LoginAsync(account, password);
            if (response==null||response.Error!=ErrorCode.ERR_Success)
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
            if (r2CGetServerInfos==null||r2CGetServerInfos.Error!=ErrorCode.ERR_Success)
            {
                Log.Error("请求服务器列表失败");
                return;
            }

            Log.Info("服务器列表有:"+r2CGetServerInfos.ServerInfoList.Count);

            ServerInfoProto serverInfoProto = r2CGetServerInfos.ServerInfoList[0];
            //获得区服角色列表
            C2R_GetRoles c2RGetRoles = C2R_GetRoles.Create();
            c2RGetRoles.Token = token;
            c2RGetRoles.AccountName = account;
            c2RGetRoles.ServerId = serverInfoProto.Id;
            R2C_GetRoles r2CGetRoles=await clientSenderComponent.Call(c2RGetRoles) as R2C_GetRoles;
            if (r2CGetRoles==null||r2CGetRoles.Error!=ErrorCode.ERR_Success)
            {
                Log.Error("请求区服角色列表失败");
                return;
            }

            RoleInfoProto roleInfoProto = default;
            if (r2CGetRoles.RoleInfoList.Count<=0)
            {
                //无角色那么创建角色
                C2R_CreateRole c2RCreateRole = C2R_CreateRole.Create();
                c2RCreateRole.Token = token;
                c2RCreateRole.ServerId = serverInfoProto.Id;
                c2RCreateRole.AccountName = account;
                c2RCreateRole.Name = account;
                R2C_CreateRole r2CCreateRole=await clientSenderComponent.Call(c2RCreateRole) as R2C_CreateRole;
                if (r2CCreateRole==null||r2CCreateRole.Error!=ErrorCode.ERR_Success)
                {
                    Log.Error($"创建区服角色失败{r2CCreateRole.Error}");
                    return;
                }

                roleInfoProto = r2CCreateRole.RoleInfo;
            }
            else
            {
                roleInfoProto = r2CGetRoles.RoleInfoList[0];
            }
            
            //请求获得realmkey
            C2R_GetRealmKey c2RGetRealmKey = C2R_GetRealmKey.Create();
            c2RGetRealmKey.Token = token;
            c2RGetRealmKey.AccountName = account;
            c2RGetRealmKey.ServerId = serverInfoProto.Id;
            R2C_GetRealmKey r2CGetRealmKey=await clientSenderComponent.Call(c2RGetRealmKey) as R2C_GetRealmKey;
            if (r2CGetRealmKey==null||r2CGetRealmKey.Error!=ErrorCode.ERR_Success)
            {
                Log.Error("获取RealmKey失败");
                return;
            }
            //r2CGetRealmKey.Key 是随机64位+时间的hashcode
            var netClient2MainLoginGame=await clientSenderComponent.LoginGameAsync(account, r2CGetRealmKey.Key, roleInfoProto.Id, r2CGetRealmKey.Address);
            if (netClient2MainLoginGame==null||netClient2MainLoginGame.Error!=ErrorCode.ERR_Success)
            {
                Log.Error($"进入游戏失败;{netClient2MainLoginGame.Error}");
                return;
            }
            
            Log.Info("进入游戏成功");
            await EventSystem.Instance.PublishAsync(root, new LoginFinish());
            //请求角色进入map地图
            //Log.Info(r2CGetRealmKey.GateId+"______________________");

            /*
            root.GetComponent<PlayerComponent>().MyId = response.PlayerId;
            //登录完成
            await EventSystem.Instance.PublishAsync(root, new LoginFinish());*/
        }
        
        /// <summary>
        /// 登录并拉取区服列表，成功时将账号、Token、区服列表写入 <see cref="NetworkCacheComponent"/>。
        /// </summary>
        /// <returns>是否成功写入缓存。</returns>
        public static async ETTask<bool> LoginGetServerList(Scene root, string account, string password)
        {
            NetworkCacheComponent cache = root.GetComponent<NetworkCacheComponent>();
            if (cache == null)
            {
                Log.Error("LoginGetServerList: 未找到 NetworkCacheComponent");
                return false;
            }

            root.RemoveComponent<ClientSenderComponent>();

            ClientSenderComponent clientSenderComponent = root.AddComponent<ClientSenderComponent>();
            var response = await clientSenderComponent.LoginAsync(account, password);
            if (response == null || response.Error != ErrorCode.ERR_Success)
            {
                Log.Error($"登录失败{response?.Error}");
                return false;
            }

            string token = response.Token;
            C2R_GetServerInfos c2RGetServerInfos = C2R_GetServerInfos.Create();
            c2RGetServerInfos.Token = token;
            c2RGetServerInfos.AccountName = account;

            R2C_GetServerInfos r2CGetServerInfos = await clientSenderComponent.Call(c2RGetServerInfos) as R2C_GetServerInfos;
            if (r2CGetServerInfos == null || r2CGetServerInfos.Error != ErrorCode.ERR_Success)
            {
                Log.Error("请求服务器列表失败");
                return false;
            }

            cache.SetLoginServerListData(account, token, r2CGetServerInfos);
            return true;
        }
        
        public static async ETTask<bool> LoginRoleEnterGame(Scene root, int serverId,string account,string token)
        {
            ClientSenderComponent clientSenderComponent = root.GetComponent<ClientSenderComponent>();
            //获得区服角色列表
            C2R_GetRoles c2RGetRoles = C2R_GetRoles.Create();
            c2RGetRoles.Token = token;
            c2RGetRoles.AccountName = account;
            c2RGetRoles.ServerId = serverId;
            R2C_GetRoles r2CGetRoles=await clientSenderComponent.Call(c2RGetRoles) as R2C_GetRoles;
            if (r2CGetRoles==null||r2CGetRoles.Error!=ErrorCode.ERR_Success)
            {
                Log.Error("请求区服角色列表失败");
                return false;
            }

            RoleInfoProto roleInfoProto = default;
            if (r2CGetRoles.RoleInfoList.Count<=0)
            {
                //无角色那么创建角色
                C2R_CreateRole c2RCreateRole = C2R_CreateRole.Create();
                c2RCreateRole.Token = token;
                c2RCreateRole.ServerId = serverId;
                c2RCreateRole.AccountName = account;
                c2RCreateRole.Name = account;
                R2C_CreateRole r2CCreateRole=await clientSenderComponent.Call(c2RCreateRole) as R2C_CreateRole;
                if (r2CCreateRole==null||r2CCreateRole.Error!=ErrorCode.ERR_Success)
                {
                    Log.Error($"创建区服角色失败{r2CCreateRole.Error}");
                    return false;
                }

                roleInfoProto = r2CCreateRole.RoleInfo;
            }
            else
            {
                roleInfoProto = r2CGetRoles.RoleInfoList[0];
            }
            
            //请求获得realmkey
            C2R_GetRealmKey c2RGetRealmKey = C2R_GetRealmKey.Create();
            c2RGetRealmKey.Token = token;
            c2RGetRealmKey.AccountName = account;
            c2RGetRealmKey.ServerId = serverId;
            R2C_GetRealmKey r2CGetRealmKey=await clientSenderComponent.Call(c2RGetRealmKey) as R2C_GetRealmKey;
            if (r2CGetRealmKey==null||r2CGetRealmKey.Error!=ErrorCode.ERR_Success)
            {
                Log.Error("获取RealmKey失败");
                return false;
            }
            //r2CGetRealmKey.Key 是随机64位+时间的hashcode
            var netClient2MainLoginGame=await clientSenderComponent.LoginGameAsync(account, r2CGetRealmKey.Key, roleInfoProto.Id, r2CGetRealmKey.Address);
            if (netClient2MainLoginGame==null||netClient2MainLoginGame.Error!=ErrorCode.ERR_Success)
            {
                Log.Error($"进入游戏失败;{netClient2MainLoginGame.Error}");
                return false;
            }

            NetworkCacheComponent netCache = root.GetComponent<NetworkCacheComponent>();
            if (netCache != null)
            {
                netCache.LoginGamePlayerId = netClient2MainLoginGame.PlayerId;
            }
            
            Log.Info("进入游戏成功");
            await EventSystem.Instance.PublishAsync(root, new LoginFinish());
            //请求角色进入map地图
            //Log.Info(r2CGetRealmKey.GateId+"______________________");

            /*
            root.GetComponent<PlayerComponent>().MyId = response.PlayerId;
            //登录完成
            await EventSystem.Instance.PublishAsync(root, new LoginFinish());*/
            return true;
        } 
        
    }
}