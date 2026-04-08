using UnityEngine;
using UnityEngine.UI;
using GameLogic;
using TEngine;

namespace ET.Client
{
	[EntitySystemOf(typeof(UILoginComponent))]
	[FriendOf(typeof(UILoginComponent))]
	public static partial class UILoginComponentSystem
	{
		private static NetworkCacheComponent NetCache(this UILoginComponent self)
		{
			return self.Root().GetComponent<NetworkCacheComponent>();
		}

		[EntitySystem]
		private static void Awake(this UILoginComponent self)
		{
			UIBindComponent m_bindComponent = self.GetParent<UI>().GameObject.GetComponent<UIBindComponent>();
			self.m_goObj1 = m_bindComponent.GetComponent<RectTransform>(0).gameObject;
			self.m_inputAccount = m_bindComponent.GetComponent<InputField>(1);
			self.m_inputPassword = m_bindComponent.GetComponent<InputField>(2);
			self.m_btnLogin = m_bindComponent.GetComponent<Button>(3);
			self.m_btnLogin.onClick.AddListener(() => { self.OnLogin(); });
			self.m_goObj2 = m_bindComponent.GetComponent<RectTransform>(4).gameObject;
			self.m_loopListVerticalScroll = m_bindComponent.GetComponent<LayoutLoopList>(5);
			self.m_btnBack = m_bindComponent.GetComponent<Button>(6);
			self.m_btnBack.onClick.AddListener(() => { self.OnBack(); });
			self.m_textServerList = m_bindComponent.GetComponent<Text>(7);
			self.m_goObj3 = m_bindComponent.GetComponent<RectTransform>(8).gameObject;
			
			self.m_loopListVerticalScroll.OnItemRefresh.RemoveAllListeners();
			self.m_loopListVerticalScroll.OnItemRefresh.AddListener((com, index) => { OnServerListItemRefresh(self, com, index); });

			self.ChangeStep1();
		}

		[EntitySystem]
		private static void Destroy(this UILoginComponent self)
		{
			if (self.m_loopListVerticalScroll != null)
			{
				self.m_loopListVerticalScroll.OnItemRefresh.RemoveAllListeners();
			}
		}

		private static void OnServerListItemRefresh(UILoginComponent self, Component com, int dataIndex)
		{
			R2C_GetServerInfos serverList = self.NetCache()?.LastServerListResponse;
			if (self.IsDisposed || serverList == null || serverList.ServerInfoList == null)
			{
				return;
			}

			var list = serverList.ServerInfoList;
			if (dataIndex < 0 || dataIndex >= list.Count)
			{
				return;
			}

			UIBindComponent itemBind = com as UIBindComponent ?? com.GetComponent<UIBindComponent>();
			if (itemBind == null)
			{
				return;
			}

			Text textInfo = itemBind.GetComponent<Text>(0);
			if (textInfo == null)
			{
				return;
			}

			string name = list[dataIndex].ServerName;
			if (list[dataIndex].Id==1)
			{
				textInfo.text = "铁炉堡";
			}else if (list[dataIndex].Id==2)
			{
				textInfo.text = "亡灵城";
			}

			RegisterServerListItemClick(self, itemBind.gameObject, dataIndex);
		}

		private static void RegisterServerListItemClick(UILoginComponent self, GameObject itemRoot, int dataIndex)
		{
			Button clickBtn = itemRoot.GetComponent<Button>();
			if (clickBtn == null)
			{
				clickBtn = itemRoot.AddComponent<Button>();
				Graphic graphic = itemRoot.GetComponent<Graphic>();
				if (graphic != null)
				{
					clickBtn.targetGraphic = graphic;
				}
			}

			clickBtn.onClick.RemoveAllListeners();
			int index = dataIndex;
			clickBtn.onClick.AddListener(() => { self.OnServerListItemClick(index); });
		}

		/// <summary>区服列表项点击，index 为 ServerInfoList 下标。</summary>
		public static void OnServerListItemClick(this UILoginComponent self, int index)
		{
			if (self.IsDisposed)
			{
				return;
			}

			self.LoginRoleEnterGameAsync(index).Coroutine();
		}

		private static async ETTask LoginRoleEnterGameAsync(this UILoginComponent self,int index)
		{
			NetworkCacheComponent cache = self.NetCache();
			R2C_GetServerInfos serverList = cache?.LastServerListResponse;
			if (serverList?.ServerInfoList == null || index < 0 || index >= serverList.ServerInfoList.Count)
			{
				return;
			}

			int serverId = serverList.ServerInfoList[index].Id;
			bool isOk = await LoginHelper.LoginRoleEnterGame(self.Root(), serverId, cache.Account, cache.Token);
			if (!isOk)
			{
				self.ChangeStep1();
			}
			else
			{
				Log.Info($"UILogin 区服列表点击 index={index}");
			}
		}

		public static void OnBack(this UILoginComponent self)
		{
			self.ChangeStep1();	
		}

		//按钮点击登录流程
		public static void OnLogin(this UILoginComponent self)
		{
			self.LoginGetServerListAsync().Coroutine();
		}

		private static async ETTask LoginGetServerListAsync(this UILoginComponent self)
		{
			bool ok = await LoginHelper.LoginGetServerList(
				self.Root(),
				self.m_inputAccount.text,
				self.m_inputPassword.text);
			if (!ok)
			{
				self.ChangeStep1();//失败返回登录
				return;
			}

			self.ChangeStep2();
		}

		//正常登录显示
		private static void ChangeStep1(this UILoginComponent self)
		{
			self.NetCache()?.ClearCache();
			self.m_goObj1.gameObject.SetActive(true);
			self.m_goObj2.gameObject.SetActive(false);
			self.m_goObj3.gameObject.SetActive(false);

		}
		
		//正常登录显示
		private static void ChangeStep2(this UILoginComponent self)
		{
			self.m_goObj1.gameObject.SetActive(false);
			self.m_goObj2.gameObject.SetActive(true);
			self.m_goObj3.gameObject.SetActive(false);
			self.RefreshServerList();
		}

		private static void RefreshServerList(this UILoginComponent self)
		{
			if (self.m_loopListVerticalScroll == null)
			{
				return;
			}

			R2C_GetServerInfos serverList = self.NetCache()?.LastServerListResponse;
			if (serverList?.ServerInfoList == null)
			{
				self.m_loopListVerticalScroll.RefreshDataCount(0);
				return;
			}

			int count = serverList.ServerInfoList.Count;
			self.m_loopListVerticalScroll.RefreshDataCount(count);
		}
	}
}
