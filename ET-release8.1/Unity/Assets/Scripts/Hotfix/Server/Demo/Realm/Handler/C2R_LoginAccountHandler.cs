using System;
using System.Net;
using System.Text.RegularExpressions;

namespace ET.Server
{
    [FriendOf(typeof(Account))]
    [MessageSessionHandler(SceneType.Realm)]
    public class C2R_LoginAccountHandler: MessageSessionHandler<C2R_LoginAccount, R2C_LoginAccount>
    {
        protected override async ETTask Run(Session session, C2R_LoginAccount request, R2C_LoginAccount response)
        {
            session.RemoveComponent<SessionAcceptTimeoutComponent>();
            //防止同一个session 发送两条一样的 C2R_LoginAccount 重复请求返回
            if (session.GetComponent<SessionLockingComponent>()!=null)
            {
                response.Error = ErrorCode.ERR_RequestRepeatedly;
                session.Disconnect().Coroutine();
                return;
            }

            if (string.IsNullOrEmpty(request.AccountName))//||string.IsNullOrEmpty(request.Password) 测试阶段password可以是empty
            {
                response.Error = ErrorCode.ERR_LoginInfoNull;
                session.Disconnect().Coroutine();
                return;
            }
            
            //这里可能需要对account正则处理，可能是手机号或者其他
            //6-15 包含大写英文，小写英文和数字
            string accountRegex = @"^[a-zA-Z0-9]{6,15}$";// @"^(?=.*[0-9].*)(?=.*[A-Z].*)(?=.*[a-z].*).{6,15}$";
            if (!Regex.IsMatch(request.AccountName.Trim(),accountRegex))
            {
                response.Error = ErrorCode.ERR_AccountFormError;
                session.Disconnect().Coroutine();
                return;
            }
            //密码正则
            string passwordRegex = @"^[A-Za-z0-9]+$";
            
            
            //携程锁 锁住这个account账号
            var coroutineLockComponent = session?.Root().GetComponent<CoroutineLockComponent>();
            using (session.AddComponent<SessionLockingComponent>())//using 自动释放
            {
                using (await coroutineLockComponent.Wait(CoroutineLockType.LoginAccount,request.AccountName.GetLongHashCode()))
                {
                    if (session.IScene==null)//登录这个session可能清空 但是其他不用加这个
                    {
                        response.Error = ErrorCode.ERR_RequestRepeatedly;
                        session.Disconnect().Coroutine();
                        return;
                    }
                    else
                    {
                        
                        //数据库操作
                        DBComponent dbComponent = session?.Root().GetComponent<DBManagerComponent>().GetZoneDB(session.Zone());
                        var accountList=await dbComponent.Query<Account>(accountInfo => accountInfo.AccountName == request.AccountName);
                        Account account = null;
                        if (accountList!=null&&accountList.Count>0)
                        {
                            account = accountList[0];
                            if (session.IScene!=null)
                            {
                                session.AddChild(account);//每个元素可控 保证session释放 account也释放
                            }
                            if (account.AccountType==(int)AccountType.BlackList)//黑名单
                            {
                                response.Error = ErrorCode.ERR_AccountInBlackListError;
                                session?.Disconnect().Coroutine();
                                account?.Dispose();//习惯 可以不写 等session释放
                                return;
                            }

                            if (!account.Password.Equals(request.Password))//密码不对
                            {
                                response.Error = ErrorCode.ERR_LoginPasswordError;
                                session?.Disconnect().Coroutine();
                                account?.Dispose();
                                return;
                            }
                            
                            account.LastLoginTime=TimeInfo.Instance.ServerNow();//保存最后一次登录时间
                            await dbComponent.Save<Account>(account);
                        }
                        else
                        {
                            //注册逻辑
                            account=session.AddChild<Account>();
                            account.AccountName = request.AccountName.Trim();
                            account.Password = request.Password;
                            account.AccountType = (int)AccountType.General;
                            long nowTime= TimeInfo.Instance.ServerNow();
                            account.CreateTime = nowTime;
                            account.LastLoginTime = nowTime;
                            await dbComponent.Save<Account>(account);
                            
                        }
                        if (session.IScene==null)
                        {
                            response.Error = ErrorCode.ERR_RequestRepeatedly;
                            session?.Disconnect().Coroutine();
                            account?.Dispose();
                            return;
                        } 
                        //开始向登录服正式登录LoginCenter
                        R2L_LoginAccountRequest r2LLoginAccountRequest = R2L_LoginAccountRequest.Create();
                        r2LLoginAccountRequest.AccountName = request.AccountName;

                        StartSceneConfig loginCenterConfig = StartSceneConfigCategory.Instance.LoginCenterConfig;

                        var loginAccountResponse = await session.Fiber().Root.GetComponent<MessageSender>().
                                Call(loginCenterConfig.ActorId,r2LLoginAccountRequest)as L2R_LoginAccountResponse;
                        if (loginAccountResponse.Error!=ErrorCode.ERR_Success)//登录服连接失败
                        {
                            response.Error = loginAccountResponse.Error;
                            session?.Disconnect().Coroutine();
                            account?.Dispose();
                            return;
                        }

                        Session otherSession = session.Root().GetComponent<AccountSessionsComponent>().Get(request.AccountName);
                        otherSession?.Send(A2C_Disconnet.Create());//存在旧的session断开连接 //0踢下线 1超时
                        otherSession?.Disconnect().Coroutine();
                        //新的session添加
                        session.Root().GetComponent<AccountSessionsComponent>().Add(request.AccountName, session);
                        //10分钟自动断开
                        session.AddComponent<AccountChectOutTimeComponent, string>(request.AccountName);
                        
                        //一个新的token
                        string token = TimeInfo.Instance.ServerNow() + RandomGenerator.RandomNumber(int.MinValue, int.MaxValue).ToString();
                        session.Root().GetComponent<TokenComponent>().Remove(request.AccountName);
                        session.Root().GetComponent<TokenComponent>().Add(request.AccountName, token);

                        response.Token = token;
                        account?.Dispose();
                        
                    }//else



                }//using
            }//using


            await ETTask.CompletedTask;
        }
        

        
    }
    
    
}
