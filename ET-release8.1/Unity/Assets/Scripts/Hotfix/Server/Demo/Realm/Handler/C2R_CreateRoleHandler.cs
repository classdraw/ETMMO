namespace ET.Server
{
    [MessageSessionHandler(SceneType.Realm)]
    [FriendOf(typeof(RoleInfo))]
    public class C2R_CreateRoleHandler:MessageSessionHandler<C2R_CreateRole,R2C_CreateRole>
    {
        protected override async ETTask Run(Session session, C2R_CreateRole request, R2C_CreateRole response)
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

            if (string.IsNullOrEmpty(request.Name))
            {
                response.Error = ErrorCode.ERR_RoleNameNull;
                //这里不需要断开
                return;
            }

            //携程锁 锁住这个account账号
            var coroutineLockComponent = session?.Root().GetComponent<CoroutineLockComponent>();
            using (session.AddComponent<SessionLockingComponent>()) //using 自动释放
            using (await coroutineLockComponent.Wait(CoroutineLockType.CreateRole, request.AccountName.GetLongHashCode()))
            {
                DBComponent dbComponent = session?.Root().GetComponent<DBManagerComponent>().GetZoneDB(session.Zone());
                var roleInfos = await dbComponent.Query<RoleInfo>(
                    d=>
                            d.Name==request.Name&&d.ServerId==request.ServerId);
                if (roleInfos!=null&&roleInfos.Count>0)
                {
                    response.Error = ErrorCode.ERR_RoleNameSame;
                    //这里不需要断开
                    return;
                }

                RoleInfo roleInfo = session.AddChild<RoleInfo>();
                roleInfo.ServerId = request.ServerId;
                roleInfo.AccountName = request.AccountName;
                roleInfo.State = (int)RoleInfoState.Normal;
                long nowTime= TimeInfo.Instance.ServerNow();
                roleInfo.CreateTime = nowTime;
                roleInfo.LastLoginTime = 0;
                roleInfo.Name = request.Name;
                roleInfo.BaseAvatar = request.BaseAvatar;

                await dbComponent.Save<RoleInfo>(roleInfo);
                    
                response.RoleInfo = roleInfo.ToMessage();
                roleInfo?.Dispose();
            }
        }
    }
}

