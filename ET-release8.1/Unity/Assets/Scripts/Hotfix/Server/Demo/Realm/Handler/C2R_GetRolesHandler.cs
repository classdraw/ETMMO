using System;
using System.Net;
using Sirenix.OdinInspector;

namespace ET.Server
{
    [MessageSessionHandler(SceneType.Realm)]
    [FriendOf(typeof(RoleInfo))]
    public class C2R_GetRolesHandler:MessageSessionHandler<C2R_GetRoles,R2C_GetRoles>
    {
        protected override async ETTask Run(Session session, C2R_GetRoles request, R2C_GetRoles response)
        {
            if (session.GetComponent<SessionLockingComponent>()!=null)
            {
                response.Error = ErrorCode.ERR_RequestRepeatedly;
                session.Disconnect().Coroutine();
                return;
            }

            string token = session.Root().GetComponent<TokenComponent>().Get(request.AccountName);
            if (token==null||token!=request.Token)
            {
                response.Error = ErrorCode.ERR_TokenError;
                session?.Disconnect().Coroutine();
                return;
            }
            
            //携程锁 锁住这个account账号
            var coroutineLockComponent = session?.Root().GetComponent<CoroutineLockComponent>();
            using (session.AddComponent<SessionLockingComponent>()) //using 自动释放
            {
                using (await coroutineLockComponent.Wait(CoroutineLockType.CreateRole, request.AccountName.GetLongHashCode()))
                {
                    DBComponent dbComponent = session.Root().GetComponent<DBManagerComponent>().GetZoneDB(session.Zone());
                    var roleInfos = await dbComponent.Query<RoleInfo>(
                        d=>
                                d.AccountName==request.AccountName&&
                                d.ServerId==request.ServerId&&
                                d.State==(int)RoleInfoState.Normal);
                    if (roleInfos==null||roleInfos.Count==0)
                    {
                        return;
                    }

                    foreach (var roleInfo in roleInfos)
                    {
                        response.RoleInfoList.Add(roleInfo.ToMessage());
                        roleInfo?.Dispose();
                    }
                    roleInfos.Clear();
                }//using
            }
        }
    }
}

