using System;
using System.Net;

namespace ET.Server
{
    [FriendOf(typeof(ServerInfoManagerComponent ))]
    [MessageSessionHandler(SceneType.Realm)]
    public class C2R_GetServerInfosHandler:MessageSessionHandler<C2R_GetServerInfos,R2C_GetServerInfos>
    {
        protected override async ETTask Run(Session session, C2R_GetServerInfos request, R2C_GetServerInfos response)
        {
            string token = session.Root().GetComponent<TokenComponent>().Get(request.AccountName);
            if (token==null||token!=request.Token)
            {
                response.Error = ErrorCode.ERR_TokenError;
                session?.Disconnect().Coroutine();
                return;
            }

            var serverInfoManager = session.Root().GetComponent<ServerInfoManagerComponent>();
            foreach (var serverInfoRef in serverInfoManager.ServerInfos)
            {
                ServerInfo serverInfo = serverInfoRef;
                response.ServerInfoList.Add(serverInfo.ToMessage());
            }
            
           await ETTask.CompletedTask;
           
        }
    }
}

