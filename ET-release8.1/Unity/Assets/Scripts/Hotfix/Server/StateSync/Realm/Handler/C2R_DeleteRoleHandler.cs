namespace ET.Server
{
    [MessageSessionHandler(SceneType.Realm)]
    [FriendOf(typeof(RoleInfo))]
    public class C2R_DeleteRoleHandler:MessageSessionHandler<C2R_DeleteRole,R2C_DeleteRole>
    {
        protected override async ETTask Run(Session session, C2R_DeleteRole request, R2C_DeleteRole response)
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
            using (await coroutineLockComponent.Wait(CoroutineLockType.CreateRole, request.AccountName.GetLongHashCode()))
            {
                DBComponent dbComponent = session?.Root().GetComponent<DBManagerComponent>().GetZoneDB(session.Zone());
                var roleInfos = await dbComponent.Query<RoleInfo>(
                    d=>d.Id==request.RoleInfoId&&
                            d.ServerId==request.ServerId);
                if (roleInfos==null||roleInfos.Count<=0)
                {
                    response.Error = ErrorCode.ERR_RoleNotExist;
                    return;
                }
                //理论上就一个，多个不用考虑
                var roleInfo = roleInfos[0];
                session.AddChild(roleInfo);
                    
                roleInfo.State = (int)RoleInfoState.Freeze;
                await dbComponent.Save<RoleInfo>(roleInfo);
                response.DeleteRoleInfoId = roleInfo.Id;
                roleInfo?.Dispose();
            }


        }
    }
}

