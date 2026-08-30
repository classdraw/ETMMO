using System;
using System.Collections.Generic;
using ET;
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

		private static RoleTextureComponent RoleTextures(this UILoginComponent self)
		{
			return self.Root().GetComponent<RoleTextureComponent>();
		}

		[EntitySystem]
		private static void Awake(this UILoginComponent self)
		{
			UIBindComponent m_bindComponent = self.GetParent<UI>().GameObject.GetComponent<UIBindComponent>();
			self.m_goObj1 = m_bindComponent.GetGameObject(0);
			self.m_inputAccount = m_bindComponent.GetComponent<InputField>(1);
			self.m_inputPassword = m_bindComponent.GetComponent<InputField>(2);
			self.m_btnLogin = m_bindComponent.GetComponent<Button>(3);
			self.m_btnLogin.onClick.AddListener(() => { self.OnLogin(); });
			self.m_goObj2 = m_bindComponent.GetGameObject(4);
			self.m_loopListVerticalScroll = m_bindComponent.GetComponent<LayoutLoopList>(5);
			self.m_btnBack1 = m_bindComponent.GetComponent<Button>(6);
			self.m_btnBack1.onClick.AddListener(() => { self.OnBack1(); });
			self.m_textServerList = m_bindComponent.GetComponent<Text>(7);
			self.m_goObj3 = m_bindComponent.GetGameObject(8);
			self.m_textLeftTitle = m_bindComponent.GetComponent<Text>(9);
			self.m_textLeftModel = m_bindComponent.GetComponent<Text>(10);
			self.m_goLeft = m_bindComponent.GetGameObject(11);
			self.m_btnLeftCreate = m_bindComponent.GetComponent<Button>(12);
			self.m_btnLeftCreate.onClick.AddListener(() => { self.OnLeftCreate(); });
			self.m_btnLeftDelete = m_bindComponent.GetComponent<Button>(13);
			self.m_btnLeftDelete.onClick.AddListener(() => { self.OnLeftDelete(); });
			self.m_btnLeftEnter = m_bindComponent.GetComponent<Button>(14);
			self.m_btnLeftEnter.onClick.AddListener(() => { self.OnLeftEnter(); });
			self.m_inputLeft = m_bindComponent.GetComponent<InputField>(15);
			self.m_textRightTitle = m_bindComponent.GetComponent<Text>(16);
			self.m_textRightModel = m_bindComponent.GetComponent<Text>(17);
			self.m_goRight = m_bindComponent.GetGameObject(18);
			self.m_btnRightCreate = m_bindComponent.GetComponent<Button>(19);
			self.m_btnRightCreate.onClick.AddListener(() => { self.OnRightCreate(); });
			self.m_btnRightDelete = m_bindComponent.GetComponent<Button>(20);
			self.m_btnRightDelete.onClick.AddListener(() => { self.OnRightDelete(); });
			self.m_btnRightEnter = m_bindComponent.GetComponent<Button>(21);
			self.m_btnRightEnter.onClick.AddListener(() => { self.OnRightEnter(); });
			self.m_inputRight = m_bindComponent.GetComponent<InputField>(22);
			self.m_btnBack2 = m_bindComponent.GetComponent<Button>(23);
			self.m_btnBack2.onClick.AddListener(() => { self.OnBack2(); });
			self.m_goLeftChoose = m_bindComponent.GetGameObject(24);
			self.m_textRaceL = m_bindComponent.GetComponent<Text>(25);
			self.m_btnRaceRightL = m_bindComponent.GetComponent<Button>(26);
			self.m_btnRaceRightL.onClick.AddListener(() => { self.OnRaceRightL(); });
			self.m_btnRaceLeftL = m_bindComponent.GetComponent<Button>(27);
			self.m_btnRaceLeftL.onClick.AddListener(() => { self.OnRaceLeftL(); });
			self.m_textGenderL = m_bindComponent.GetComponent<Text>(28);
			self.m_btnGenderRightL = m_bindComponent.GetComponent<Button>(29);
			self.m_btnGenderRightL.onClick.AddListener(() => { self.OnGenderRightL(); });
			self.m_btnGenderLeftL = m_bindComponent.GetComponent<Button>(30);
			self.m_btnGenderLeftL.onClick.AddListener(() => { self.OnGenderLeftL(); });
			self.m_textBodyL = m_bindComponent.GetComponent<Text>(31);
			self.m_btnBodyRightL = m_bindComponent.GetComponent<Button>(32);
			self.m_btnBodyRightL.onClick.AddListener(() => { self.OnBodyRightL(); });
			self.m_btnBodyLeftL = m_bindComponent.GetComponent<Button>(33);
			self.m_btnBodyLeftL.onClick.AddListener(() => { self.OnBodyLeftL(); });
			self.m_textHeadL = m_bindComponent.GetComponent<Text>(34);
			self.m_btnHeadRightL = m_bindComponent.GetComponent<Button>(35);
			self.m_btnHeadRightL.onClick.AddListener(() => { self.OnHeadRightL(); });
			self.m_btnHeadLeftL = m_bindComponent.GetComponent<Button>(36);
			self.m_btnHeadLeftL.onClick.AddListener(() => { self.OnHeadLeftL(); });
			self.m_textTailL = m_bindComponent.GetComponent<Text>(37);
			self.m_btnTailRightL = m_bindComponent.GetComponent<Button>(38);
			self.m_btnTailRightL.onClick.AddListener(() => { self.OnTailRightL(); });
			self.m_btnTailLeftL = m_bindComponent.GetComponent<Button>(39);
			self.m_btnTailLeftL.onClick.AddListener(() => { self.OnTailLeftL(); });
			self.m_textShirtL = m_bindComponent.GetComponent<Text>(40);
			self.m_btnShirtRightL = m_bindComponent.GetComponent<Button>(41);
			self.m_btnShirtRightL.onClick.AddListener(() => { self.OnShirtRightL(); });
			self.m_btnShirtLeftL = m_bindComponent.GetComponent<Button>(42);
			self.m_btnShirtLeftL.onClick.AddListener(() => { self.OnShirtLeftL(); });
			self.m_textPantsL = m_bindComponent.GetComponent<Text>(43);
			self.m_btnPantsRightL = m_bindComponent.GetComponent<Button>(44);
			self.m_btnPantsRightL.onClick.AddListener(() => { self.OnPantsRightL(); });
			self.m_btnPantsLeftL = m_bindComponent.GetComponent<Button>(45);
			self.m_btnPantsLeftL.onClick.AddListener(() => { self.OnPantsLeftL(); });
			self.m_goRightChoose = m_bindComponent.GetGameObject(46);
			self.m_textRaceR = m_bindComponent.GetComponent<Text>(47);
			self.m_btnRaceRightR = m_bindComponent.GetComponent<Button>(48);
			self.m_btnRaceRightR.onClick.AddListener(() => { self.OnRaceRightR(); });
			self.m_btnRaceLeftR = m_bindComponent.GetComponent<Button>(49);
			self.m_btnRaceLeftR.onClick.AddListener(() => { self.OnRaceLeftR(); });
			self.m_textGenderR = m_bindComponent.GetComponent<Text>(50);
			self.m_btnGenderRightR = m_bindComponent.GetComponent<Button>(51);
			self.m_btnGenderRightR.onClick.AddListener(() => { self.OnGenderRightR(); });
			self.m_btnGenderLeftR = m_bindComponent.GetComponent<Button>(52);
			self.m_btnGenderLeftR.onClick.AddListener(() => { self.OnGenderLeftR(); });
			self.m_textBodyR = m_bindComponent.GetComponent<Text>(53);
			self.m_btnBodyRightR = m_bindComponent.GetComponent<Button>(54);
			self.m_btnBodyRightR.onClick.AddListener(() => { self.OnBodyRightR(); });
			self.m_btnBodyLeftR = m_bindComponent.GetComponent<Button>(55);
			self.m_btnBodyLeftR.onClick.AddListener(() => { self.OnBodyLeftR(); });
			self.m_textHeadR = m_bindComponent.GetComponent<Text>(56);
			self.m_btnHeadRightR = m_bindComponent.GetComponent<Button>(57);
			self.m_btnHeadRightR.onClick.AddListener(() => { self.OnHeadRightR(); });
			self.m_btnHeadLeftR = m_bindComponent.GetComponent<Button>(58);
			self.m_btnHeadLeftR.onClick.AddListener(() => { self.OnHeadLeftR(); });
			self.m_textTailR = m_bindComponent.GetComponent<Text>(59);
			self.m_btnTailRightR = m_bindComponent.GetComponent<Button>(60);
			self.m_btnTailRightR.onClick.AddListener(() => { self.OnTailRightR(); });
			self.m_btnTailLeftR = m_bindComponent.GetComponent<Button>(61);
			self.m_btnTailLeftR.onClick.AddListener(() => { self.OnTailLeftR(); });
			self.m_textShirtR = m_bindComponent.GetComponent<Text>(62);
			self.m_btnShirtRightR = m_bindComponent.GetComponent<Button>(63);
			self.m_btnShirtRightR.onClick.AddListener(() => { self.OnShirtRightR(); });
			self.m_btnShirtLeftR = m_bindComponent.GetComponent<Button>(64);
			self.m_btnShirtLeftR.onClick.AddListener(() => { self.OnShirtLeftR(); });
			self.m_textPantsR = m_bindComponent.GetComponent<Text>(65);
			self.m_btnPantsRightR = m_bindComponent.GetComponent<Button>(66);
			self.m_btnPantsRightR.onClick.AddListener(() => { self.OnPantsRightR(); });
			self.m_btnPantsLeftR = m_bindComponent.GetComponent<Button>(67);
			self.m_btnPantsLeftR.onClick.AddListener(() => { self.OnPantsLeftR(); });
			self.m_goUIRoleLeft = m_bindComponent.GetGameObject(68);
			self.m_goUIRoleRight = m_bindComponent.GetGameObject(69);
			self.m_playerUIRoleLeft = self.m_goUIRoleLeft != null ? self.m_goUIRoleLeft.GetComponent<FrameSheetAnimPlayer>() : null;
			self.m_playerUIRoleRight = self.m_goUIRoleRight != null ? self.m_goUIRoleRight.GetComponent<FrameSheetAnimPlayer>() : null;

			RoleTextureComponent roleTex = self.RoleTextures();
			if (roleTex != null)
			{
				LoginRoleAppearanceHelper.InitDefault(roleTex, ref self.LeftAppearance);
				LoginRoleAppearanceHelper.InitDefault(roleTex, ref self.RightAppearance);
			}

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
		#region 点击按钮流程

		
		//按钮点击登录流程
		public static void OnLogin(this UILoginComponent self)
		{
			self.LoginGetServerListAsync().Coroutine();
		}


		public static void OnBack(this UILoginComponent self)
		{
			self.ChangeStep1();	
		}

		public static void OnBack1(this UILoginComponent self)
		{
			self.ChangeStep1();
		}

		public static void OnBack2(this UILoginComponent self)
		{
			self.ChangeStep2();
		}
		
		public static void OnLeftCreate(this UILoginComponent self)
		{
			self.OnLeftCreateAsync().Coroutine();
		}

		public static void OnLeftTran(this UILoginComponent self)
		{
			if (self.IsDisposed)
			{
				return;
			}

			int cur = self.PendingCreateLeftConfigId == 0
				? DefaultAvatarHelper.GetDefaultRoleUnitConfigId()
				: self.PendingCreateLeftConfigId;
			self.PendingCreateLeftConfigId = DefaultAvatarHelper.NextRoleUnitConfigId(cur);
			self.UpdateLeftModelBaseAvatarText();
		}

		public static void OnLeftDelete(this UILoginComponent self)
		{
			self.OnLeftDeleteAsync().Coroutine();
		}

		public static void OnLeftEnter(this UILoginComponent self)
		{
			self.OnLeftEnterAsync().Coroutine();
		}

		public static void OnRightCreate(this UILoginComponent self)
		{
			self.OnRightCreateAsync().Coroutine();
		}

		public static void OnRightTran(this UILoginComponent self)
		{
			if (self.IsDisposed)
			{
				return;
			}

			int cur = self.PendingCreateRightConfigId == 0
				? DefaultAvatarHelper.GetDefaultRoleUnitConfigId()
				: self.PendingCreateRightConfigId;
			self.PendingCreateRightConfigId = DefaultAvatarHelper.NextRoleUnitConfigId(cur);
			self.UpdateRightModelBaseAvatarText();
		}

		public static void OnRightDelete(this UILoginComponent self)
		{
			self.OnRightDeleteAsync().Coroutine();
		}

		public static void OnRightEnter(this UILoginComponent self)
		{
			self.OnRightEnterAsync().Coroutine();
		}
		
		
		/// <summary>区服列表项点击，index 为 ServerInfoList 下标。</summary>
		public static void OnServerListItemClick(this UILoginComponent self, int index)
		{
			if (self.IsDisposed)
			{
				return;
			}

			self.LoginGetRoleList(index).Coroutine();
		}
		public static void OnRaceRightL(this UILoginComponent self) => self.CycleLeftAppearanceRace(1);
		public static void OnRaceLeftL(this UILoginComponent self) => self.CycleLeftAppearanceRace(-1);
		public static void OnGenderRightL(this UILoginComponent self) => self.CycleLeftAppearanceGender(1);
		public static void OnGenderLeftL(this UILoginComponent self) => self.CycleLeftAppearanceGender(-1);
		public static void OnBodyRightL(this UILoginComponent self) => self.CycleLeftAppearancePart(FrameRolePartType.Body, 1);
		public static void OnBodyLeftL(this UILoginComponent self) => self.CycleLeftAppearancePart(FrameRolePartType.Body, -1);
		public static void OnHeadRightL(this UILoginComponent self) => self.CycleLeftAppearancePart(FrameRolePartType.Head, 1);
		public static void OnHeadLeftL(this UILoginComponent self) => self.CycleLeftAppearancePart(FrameRolePartType.Head, -1);
		public static void OnTailRightL(this UILoginComponent self) => self.CycleLeftAppearancePart(FrameRolePartType.Tail, 1);
		public static void OnTailLeftL(this UILoginComponent self) => self.CycleLeftAppearancePart(FrameRolePartType.Tail, -1);
		public static void OnShirtRightL(this UILoginComponent self) => self.CycleLeftAppearancePart(FrameRolePartType.Shirt, 1);
		public static void OnShirtLeftL(this UILoginComponent self) => self.CycleLeftAppearancePart(FrameRolePartType.Shirt, -1);
		public static void OnPantsRightL(this UILoginComponent self) => self.CycleLeftAppearancePart(FrameRolePartType.Pants, 1);
		public static void OnPantsLeftL(this UILoginComponent self) => self.CycleLeftAppearancePart(FrameRolePartType.Pants, -1);

		public static void OnRaceRightR(this UILoginComponent self) => self.CycleRightAppearanceRace(1);
		public static void OnRaceLeftR(this UILoginComponent self) => self.CycleRightAppearanceRace(-1);
		public static void OnGenderRightR(this UILoginComponent self) => self.CycleRightAppearanceGender(1);
		public static void OnGenderLeftR(this UILoginComponent self) => self.CycleRightAppearanceGender(-1);
		public static void OnBodyRightR(this UILoginComponent self) => self.CycleRightAppearancePart(FrameRolePartType.Body, 1);
		public static void OnBodyLeftR(this UILoginComponent self) => self.CycleRightAppearancePart(FrameRolePartType.Body, -1);
		public static void OnHeadRightR(this UILoginComponent self) => self.CycleRightAppearancePart(FrameRolePartType.Head, 1);
		public static void OnHeadLeftR(this UILoginComponent self) => self.CycleRightAppearancePart(FrameRolePartType.Head, -1);
		public static void OnTailRightR(this UILoginComponent self) => self.CycleRightAppearancePart(FrameRolePartType.Tail, 1);
		public static void OnTailLeftR(this UILoginComponent self) => self.CycleRightAppearancePart(FrameRolePartType.Tail, -1);
		public static void OnShirtRightR(this UILoginComponent self) => self.CycleRightAppearancePart(FrameRolePartType.Shirt, 1);
		public static void OnShirtLeftR(this UILoginComponent self) => self.CycleRightAppearancePart(FrameRolePartType.Shirt, -1);
		public static void OnPantsRightR(this UILoginComponent self) => self.CycleRightAppearancePart(FrameRolePartType.Pants, 1);
		public static void OnPantsLeftR(this UILoginComponent self) => self.CycleRightAppearancePart(FrameRolePartType.Pants, -1);
		#endregion
		
		#region Logic方法

		/// <summary>
		/// 先判断 <paramref name="result"/>.Ok；失败时除 <see cref="ErrorCode.ERR_RoleNameSame"/> 外回到登录第一步。
		/// </summary>
		private static void HandleLoginOpFailure(this UILoginComponent self, LoginOperationResult result, string tag)
		{
			if (result.Ok)
			{
				return;
			}

			Log.Info($"UILogin {tag}: 失败 ErrorCode={result.ErrorCode}");
			if (result.ErrorCode != ErrorCode.ERR_RoleNameSame)
			{
				self.ChangeStep1();
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

		private static async ETTask OnLeftCreateAsync(this UILoginComponent self)
		{
			if (self.IsDisposed)
			{
				return;
			}

			NetworkCacheComponent cache = self.NetCache();
			R2C_GetRoles roles = cache?.LastRoleListResponse;
			bool hasLeftRole = roles?.RoleInfoList != null && roles.RoleInfoList.Count >= 1;
			if (hasLeftRole)
			{
				Log.Warning("UILogin OnLeftCreate: 左侧已有角色，无法重复创建");
				return;
			}

			string roleName = self.m_inputLeft != null ? self.m_inputLeft.text.Trim() : string.Empty;
			if (string.IsNullOrEmpty(roleName))
			{
				Log.Warning("UILogin OnLeftCreate: 请输入左侧角色名称");
				return;
			}

			if (self.PendingCreateLeftConfigId == 0)
			{
				self.PendingCreateLeftConfigId = DefaultAvatarHelper.GetDefaultRoleUnitConfigId();
			}

			LoginOperationResult result = await LoginHelper.LoginCreateRole(self.Root(), roleName, self.PendingCreateLeftConfigId);
			if (!result.Ok)
			{
				self.HandleLoginOpFailure(result, "OnLeftCreate");
				return;
			}

			self.RefreshRoleList();
		}

		private static async ETTask OnRightCreateAsync(this UILoginComponent self)
		{
			if (self.IsDisposed)
			{
				return;
			}

			NetworkCacheComponent cache = self.NetCache();
			R2C_GetRoles roles = cache?.LastRoleListResponse;
			bool hasRightRole = roles?.RoleInfoList != null && roles.RoleInfoList.Count >= 2;
			if (hasRightRole)
			{
				Log.Warning("UILogin OnRightCreate: 右侧（第 2 个）已有角色，无法重复创建");
				return;
			}

			string roleName = self.m_inputRight != null ? self.m_inputRight.text.Trim() : string.Empty;
			if (string.IsNullOrEmpty(roleName))
			{
				Log.Warning("UILogin OnRightCreate: 请输入右侧角色名称");
				return;
			}

			if (self.PendingCreateRightConfigId == 0)
			{
				self.PendingCreateRightConfigId = DefaultAvatarHelper.GetDefaultRoleUnitConfigId();
			}

			LoginOperationResult result = await LoginHelper.LoginCreateRole(self.Root(), roleName, self.PendingCreateRightConfigId);
			if (!result.Ok)
			{
				self.HandleLoginOpFailure(result, "OnRightCreate");
				return;
			}

			self.RefreshRoleList();
		}

		private static async ETTask OnLeftDeleteAsync(this UILoginComponent self)
		{
			if (self.IsDisposed)
			{
				return;
			}

			NetworkCacheComponent cache = self.NetCache();
			R2C_GetRoles roles = cache?.LastRoleListResponse;
			if (roles?.RoleInfoList == null || roles.RoleInfoList.Count < 1)
			{
				Log.Warning("UILogin OnLeftDelete: 左侧无角色（index=0）");
				return;
			}

			long roleId = roles.RoleInfoList[0].Id;
			LoginOperationResult result = await LoginHelper.LoginDeleteRole(self.Root(), roleId);
			if (!result.Ok)
			{
				self.HandleLoginOpFailure(result, "OnLeftDelete");
				return;
			}

			self.RefreshRoleList();
		}

		private static async ETTask OnRightDeleteAsync(this UILoginComponent self)
		{
			if (self.IsDisposed)
			{
				return;
			}

			NetworkCacheComponent cache = self.NetCache();
			R2C_GetRoles roles = cache?.LastRoleListResponse;
			if (roles?.RoleInfoList == null || roles.RoleInfoList.Count < 2)
			{
				Log.Warning("UILogin OnRightDelete: 右侧无角色（index=1）");
				return;
			}

			long roleId = roles.RoleInfoList[1].Id;
			LoginOperationResult result = await LoginHelper.LoginDeleteRole(self.Root(), roleId);
			if (!result.Ok)
			{
				self.HandleLoginOpFailure(result, "OnRightDelete");
				return;
			}

			self.RefreshRoleList();
		}

		private static async ETTask OnLeftEnterAsync(this UILoginComponent self)
		{
			if (self.IsDisposed)
			{
				return;
			}

			R2C_GetRoles roles = self.NetCache()?.LastRoleListResponse;
			if (roles?.RoleInfoList == null || roles.RoleInfoList.Count < 1)
			{
				Log.Warning("UILogin OnLeftEnter: 左侧无角色");
				return;
			}

			long roleId = roles.RoleInfoList[0].Id;
			int configId = roles.RoleInfoList[0].ConfigId;
			string name = roles.RoleInfoList[0].Name;
			LoginOperationResult result = await LoginHelper.LoginRoleEnterGame(self.Root(), roleId, configId,name);
			if (!result.Ok)
			{
				self.HandleLoginOpFailure(result, "OnLeftEnter");
			}
		}

		private static async ETTask OnRightEnterAsync(this UILoginComponent self)
		{
			if (self.IsDisposed)
			{
				return;
			}

			R2C_GetRoles roles = self.NetCache()?.LastRoleListResponse;
			if (roles?.RoleInfoList == null || roles.RoleInfoList.Count < 2)
			{
				Log.Warning("UILogin OnRightEnter: 右侧无角色");
				return;
			}

			long roleId = roles.RoleInfoList[1].Id;
			int configId = roles.RoleInfoList[1].ConfigId;
			string name = roles.RoleInfoList[1].Name;
			LoginOperationResult result = await LoginHelper.LoginRoleEnterGame(self.Root(), roleId, configId,name);
			if (!result.Ok)
			{
				self.HandleLoginOpFailure(result, "OnRightEnter");
			}
		}

		private static async ETTask LoginGetRoleList(this UILoginComponent self,int index)
		{
			NetworkCacheComponent cache = self.NetCache();
			R2C_GetServerInfos serverList = cache?.LastServerListResponse;
			if (serverList?.ServerInfoList == null || index < 0 || index >= serverList.ServerInfoList.Count)
			{
				return;
			}

			int serverId = serverList.ServerInfoList[index].Id;
			cache.ServerId = serverId;
			LoginOperationResult result = await LoginHelper.LoginGetRoleList(self.Root());
			if (!result.Ok)
			{
				self.HandleLoginOpFailure(result, "LoginGetRoleList");
				return;
			}

			Log.Info($"UILogin 区服列表点击 index={index}");
			self.ChangeStep3();
		}

		private static async ETTask LoginGetServerListAsync(this UILoginComponent self)
		{
			LoginOperationResult result = await LoginHelper.LoginGetServerList(
				self.Root(),
				self.m_inputAccount.text,
				self.m_inputPassword.text);
			if (!result.Ok)
			{
				self.HandleLoginOpFailure(result, "LoginGetServerList");
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
		
				
		//正常登录显示
		private static void ChangeStep3(this UILoginComponent self)
		{
			self.m_goObj1.gameObject.SetActive(false);
			self.m_goObj2.gameObject.SetActive(false);
			self.m_goObj3.gameObject.SetActive(true);
			self.RefreshRoleList();
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

		private static void RefreshRoleList(this UILoginComponent self)
		{
			NetworkCacheComponent cache = self.NetCache();
			R2C_GetRoles r2CGetRoles = cache.LastRoleListResponse;
			if (r2CGetRoles?.RoleInfoList != null && r2CGetRoles.RoleInfoList.Count >= 1)
			{
				self.SetLeftRole(r2CGetRoles.RoleInfoList[0]);
			}
			else
			{
				self.SetLeftRole(null);
			}

			if (r2CGetRoles?.RoleInfoList != null && r2CGetRoles.RoleInfoList.Count >= 2)
			{
				self.SetRightRole(r2CGetRoles.RoleInfoList[1]);
			}
			else
			{
				self.SetRightRole(null);
			}
		}

		private static void UpdateLeftAppearanceText(this UILoginComponent self)
		{
			LoginRoleAppearance appearance = self.LeftAppearance;
			RoleTextureComponent roleTex = self.RoleTextures();

			if (self.m_textRaceL != null)
			{
				self.m_textRaceL.text = LoginRoleDisplayHelper.GetRaceName(appearance.Race);
			}

			if (self.m_textGenderL != null)
			{
				self.m_textGenderL.text = LoginRoleDisplayHelper.GetGenderName(appearance.Gender);
			}

			SetAppearancePartText(self.m_textBodyL, roleTex, appearance.BodyDisplayId);
			SetAppearancePartText(self.m_textHeadL, roleTex, appearance.HeadDisplayId);
			SetAppearancePartText(self.m_textTailL, roleTex, appearance.TailDisplayId);
			SetAppearancePartText(self.m_textShirtL, roleTex, appearance.ShirtDisplayId);
			SetAppearancePartText(self.m_textPantsL, roleTex, appearance.PantsDisplayId);
		}

		private static void UpdateRightAppearanceText(this UILoginComponent self)
		{
			LoginRoleAppearance appearance = self.RightAppearance;
			RoleTextureComponent roleTex = self.RoleTextures();

			if (self.m_textRaceR != null)
			{
				self.m_textRaceR.text = LoginRoleDisplayHelper.GetRaceName(appearance.Race);
			}

			if (self.m_textGenderR != null)
			{
				self.m_textGenderR.text = LoginRoleDisplayHelper.GetGenderName(appearance.Gender);
			}

			SetAppearancePartText(self.m_textBodyR, roleTex, appearance.BodyDisplayId);
			SetAppearancePartText(self.m_textHeadR, roleTex, appearance.HeadDisplayId);
			SetAppearancePartText(self.m_textTailR, roleTex, appearance.TailDisplayId);
			SetAppearancePartText(self.m_textShirtR, roleTex, appearance.ShirtDisplayId);
			SetAppearancePartText(self.m_textPantsR, roleTex, appearance.PantsDisplayId);
		}

		private static void SetAppearancePartText(Text text, RoleTextureComponent roleTex, int displayId)
		{
			if (text == null)
			{
				return;
			}

			text.text = roleTex != null
				? roleTex.GetPartDisplayName(displayId)
				: displayId.ToString();
		}

		private static void UpdateLeftModelBaseAvatarText(this UILoginComponent self)
		{
			if (self.m_textLeftModel == null)
			{
				return;
			}

			int configId = self.PendingCreateLeftConfigId == 0
				? DefaultAvatarHelper.GetDefaultRoleUnitConfigId()
				: self.PendingCreateLeftConfigId;
			self.m_textLeftModel.text = configId.ToString();
		}

		private static void UpdateRightModelBaseAvatarText(this UILoginComponent self)
		{
			if (self.m_textRightModel == null)
			{
				return;
			}

			int configId = self.PendingCreateRightConfigId == 0
				? DefaultAvatarHelper.GetDefaultRoleUnitConfigId()
				: self.PendingCreateRightConfigId;
			self.m_textRightModel.text = configId.ToString();
		}

		private static void CycleLeftAppearanceRace(this UILoginComponent self, int delta)
		{
			RoleTextureComponent roleTex = self.RoleTextures();
			if (roleTex == null)
			{
				return;
			}

			LoginRoleAppearanceHelper.CycleRace(roleTex, ref self.LeftAppearance, delta);
			self.RefreshLeftRolePreview();
		}

		private static void CycleLeftAppearanceGender(this UILoginComponent self, int delta)
		{
			RoleTextureComponent roleTex = self.RoleTextures();
			if (roleTex == null)
			{
				return;
			}

			LoginRoleAppearanceHelper.CycleGender(roleTex, ref self.LeftAppearance, delta);
			self.RefreshLeftRolePreview();
		}

		private static void CycleLeftAppearancePart(this UILoginComponent self, FrameRolePartType part, int delta)
		{
			RoleTextureComponent roleTex = self.RoleTextures();
			if (roleTex == null)
			{
				return;
			}

			LoginRoleAppearanceHelper.CyclePart(roleTex, ref self.LeftAppearance, part, delta);
			self.RefreshLeftRolePreview();
		}

		private static void CycleRightAppearanceRace(this UILoginComponent self, int delta)
		{
			RoleTextureComponent roleTex = self.RoleTextures();
			if (roleTex == null)
			{
				return;
			}

			LoginRoleAppearanceHelper.CycleRace(roleTex, ref self.RightAppearance, delta);
			self.RefreshRightRolePreview();
		}

		private static void CycleRightAppearanceGender(this UILoginComponent self, int delta)
		{
			RoleTextureComponent roleTex = self.RoleTextures();
			if (roleTex == null)
			{
				return;
			}

			LoginRoleAppearanceHelper.CycleGender(roleTex, ref self.RightAppearance, delta);
			self.RefreshRightRolePreview();
		}

		private static void CycleRightAppearancePart(this UILoginComponent self, FrameRolePartType part, int delta)
		{
			RoleTextureComponent roleTex = self.RoleTextures();
			if (roleTex == null)
			{
				return;
			}

			LoginRoleAppearanceHelper.CyclePart(roleTex, ref self.RightAppearance, part, delta);
			self.RefreshRightRolePreview();
		}

		private static void RefreshLeftRolePreview(this UILoginComponent self)
		{
			UILoginRolePreviewHelper.ApplyPreview(self.m_playerUIRoleLeft, self.RoleTextures(), self.LeftAppearance);
			self.UpdateLeftAppearanceText();
		}

		private static void RefreshRightRolePreview(this UILoginComponent self)
		{
			UILoginRolePreviewHelper.ApplyPreview(self.m_playerUIRoleRight, self.RoleTextures(), self.RightAppearance);
			self.UpdateRightAppearanceText();
		}

		private static void SetLeftRole(this UILoginComponent self,RoleInfoProto roleInfoProto)
		{
			self.m_inputLeft.text = "";
			if (roleInfoProto==null)
			{
				self.m_btnLeftDelete.gameObject.SetActive(false);
				self.m_btnLeftCreate.gameObject.SetActive(true);
				self.m_btnLeftEnter.gameObject.SetActive(false);
				self.m_goLeftChoose.SetActive(true);
				self.m_textLeftTitle.text = "角色1";
				self.m_inputLeft.gameObject.SetActive(true);
				self.RefreshLeftRolePreview();
			}
			else
			{
				self.m_btnLeftDelete.gameObject.SetActive(true);
				self.m_btnLeftCreate.gameObject.SetActive(false);
				self.m_btnLeftEnter.gameObject.SetActive(true);
				self.m_goLeftChoose.SetActive(false);
				self.m_inputLeft.gameObject.SetActive(false);
				self.m_textLeftTitle.text = roleInfoProto.Name;
			}
		}

		private static void SetRightRole(this UILoginComponent self, RoleInfoProto roleInfoProto)
		{
			self.m_inputRight.text = "";
			if (roleInfoProto == null)
			{
				self.m_btnRightDelete.gameObject.SetActive(false);
				self.m_btnRightCreate.gameObject.SetActive(true);
				self.m_btnRightEnter.gameObject.SetActive(false);
				self.m_goRightChoose.SetActive(true);
				self.m_textRightTitle.text = "角色2";
				self.m_inputRight.gameObject.SetActive(true);
				self.RefreshRightRolePreview();
			}
			else
			{
				self.m_btnRightDelete.gameObject.SetActive(true);
				self.m_btnRightCreate.gameObject.SetActive(false);
				self.m_btnRightEnter.gameObject.SetActive(true);
				self.m_goRightChoose.SetActive(false);
				self.m_inputRight.gameObject.SetActive(false);
				self.m_textRightTitle.text = roleInfoProto.Name;
			}
		}

		#endregion


	}
}
