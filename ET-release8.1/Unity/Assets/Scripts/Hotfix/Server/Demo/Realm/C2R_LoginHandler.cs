using System;
using System.Net;


namespace ET.Server
{
	[FriendOf(typeof(AccountInfo))]
	[MessageSessionHandler(SceneType.Realm)]
	public class C2R_LoginHandler : MessageSessionHandler<C2R_Login, R2C_Login>
	{
		protected override async ETTask Run(Session session, C2R_Login request, R2C_Login response)
		{
			if (CheckLoginValid(session,request,response)) 
			{
				//携程锁
				using(await session.Root().GetComponent<CoroutineLockComponent>().Wait(CoroutineLockType.LoginAccount,request.Account.GetLongHashCode()))
				{
					DBComponent dbComponent = session.Root().GetComponent<DBManagerComponent>().GetZoneDB(session.Zone());
					var accountList=await dbComponent.Query<AccountInfo>(accountInfo => accountInfo.Account == request.Account);
					if (accountList==null||accountList.Count==0)
					{
						AccountInfoComponent accountInfoComponent =
								session.GetComponent<AccountInfoComponent>() ?? session.AddComponent<AccountInfoComponent>();
					
						var accountInfo = accountInfoComponent.AddChild<AccountInfo>();
						accountInfo.Account = request.Account;
						accountInfo.Password=request.Password;
						await dbComponent.Save(accountInfo);
					}
					else
					{
						if (accountList[0].Password!=request.Password)
						{
							//密码错误
							response.Error = ErrorCode.ERR_LoginPwdError;
							CloseSession(session).Coroutine();
							return;
						}
					}
				}

				
				// 随机分配一个Gate(网关)
				StartSceneConfig config = RealmGateAddressHelper.GetGate(session.Zone(), request.Account);
				Log.Debug($"gate address: {config}");
			
				// 向gate请求一个key,客户端可以拿着这个key连接gate
				R2G_GetLoginKey r2GGetLoginKey = R2G_GetLoginKey.Create();
				r2GGetLoginKey.Account = request.Account;
				G2R_GetLoginKey g2RGetLoginKey = (G2R_GetLoginKey) await session.Fiber().Root.GetComponent<MessageSender>().Call(
					config.ActorId, r2GGetLoginKey);

				response.Address = config.InnerIPPort.ToString();
				response.Key = g2RGetLoginKey.Key;
				response.GateId = g2RGetLoginKey.GateId;
				CloseSession(session).Coroutine();
			}
			
		}

		private bool CheckLoginValid(Session session, C2R_Login request, R2C_Login response)
		{
			if (string.IsNullOrEmpty(request.Account))//||string.IsNullOrEmpty(request.Password) 测试阶段password可以是empty
			{
				response.Error = ErrorCode.ERR_LoginInfoEmpty;
				CloseSession(session).Coroutine();
				return false;
			}
			//这里有其他合法性校验

			return true;
		}

		private async ETTask CloseSession(Session session)
		{
			await session.Root().GetComponent<TimerComponent>().WaitAsync(1000);
			session.Dispose();
		}
	}
}
