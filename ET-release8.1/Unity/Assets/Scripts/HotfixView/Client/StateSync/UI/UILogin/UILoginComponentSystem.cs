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
			self.m_btnBack1 = m_bindComponent.GetComponent<Button>(6);
			self.m_btnBack1.onClick.AddListener(() => { self.OnBack(); });
			self.m_textServerList = m_bindComponent.GetComponent<Text>(7);
			self.m_goObj3 = m_bindComponent.GetComponent<RectTransform>(8).gameObject;
			self.m_textLeftTitle = m_bindComponent.GetComponent<Text>(9);
			self.m_goLeft = m_bindComponent.GetComponent<RectTransform>(10).gameObject;
			self.m_btnLeftCreate = m_bindComponent.GetComponent<Button>(11);
			self.m_btnLeftCreate.onClick.AddListener(() => { self.OnLeftCreate(); });
			self.m_btnLeftTran = m_bindComponent.GetComponent<Button>(12);
			self.m_btnLeftTran.onClick.AddListener(() => { self.OnLeftTran(); });
			self.m_btnLeftDelete = m_bindComponent.GetComponent<Button>(13);
			self.m_btnLeftDelete.onClick.AddListener(() => { self.OnLeftDelete(); });
			self.m_btnLeftEnter = m_bindComponent.GetComponent<Button>(14);
			self.m_btnLeftEnter.onClick.AddListener(() => { self.OnLeftEnter(); });
			self.m_inputLeft = m_bindComponent.GetComponent<InputField>(15);
			self.m_textRightTitle = m_bindComponent.GetComponent<Text>(16);
			self.m_goRight = m_bindComponent.GetComponent<RectTransform>(17).gameObject;
			self.m_btnRightCreate = m_bindComponent.GetComponent<Button>(18);
			self.m_btnRightCreate.onClick.AddListener(() => { self.OnRightCreate(); });
			self.m_btnRightTran = m_bindComponent.GetComponent<Button>(19);
			self.m_btnRightTran.onClick.AddListener(() => { self.OnRightTran(); });
			self.m_btnRightDelete = m_bindComponent.GetComponent<Button>(20);
			self.m_btnRightDelete.onClick.AddListener(() => { self.OnRightDelete(); });
			self.m_btnRightEnter = m_bindComponent.GetComponent<Button>(21);
			self.m_btnRightEnter.onClick.AddListener(() => { self.OnRightEnter(); });
			self.m_inputRight = m_bindComponent.GetComponent<InputField>(22);
			self.m_btnBack2 = m_bindComponent.GetComponent<Button>(23);
			self.m_btnBack2.onClick.AddListener(() => { self.OnBack(); });
			
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

			self.PendingCreateLeftBaseAvatar = DefaultAvatarHelper.NextBaseAvatar(
				self.PendingCreateLeftBaseAvatar == 0 ? DefaultAvatarHelper.GetDefaultBaseAvatar() : self.PendingCreateLeftBaseAvatar);
			//显示模型
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

			self.PendingCreateRightBaseAvatar = DefaultAvatarHelper.NextBaseAvatar(
				self.PendingCreateRightBaseAvatar == 0 ? DefaultAvatarHelper.GetDefaultBaseAvatar() : self.PendingCreateRightBaseAvatar);
			//显示模型
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

		/// <summary>
		/// 按 AvatarConfig 加载模型预制体，将其中 Sprite 同步到 <paramref name="partsCollector"/> 对应部位（逻辑同 <see cref="Avatar2DComponentSystem"/> 的 ChangeAvatar）。
		/// </summary>
		public static async ETTask ChangeAvatar(this UILoginComponent self, int avatarConfigId, ReferenceSpriteCollector partsCollector)
		{
			if (partsCollector == null)
			{
				Log.Warning("UILogin ChangeAvatar: partsCollector 为空");
				return;
			}

			if (!AvatarConfigCategory.Instance.Contain(avatarConfigId))
			{
				Log.Warning($"UILogin ChangeAvatar: AvatarConfig 不存在 id={avatarConfigId}");
				return;
			}

			ResourcesLoaderComponent resLoader = self.Scene().GetComponent<ResourcesLoaderComponent>();
			if (resLoader == null)
			{
				Log.Warning("UILogin ChangeAvatar: CurrentScene 无 ResourcesLoaderComponent");
				return;
			}

			AvatarConfig cfg = AvatarConfigCategory.Instance.Get(avatarConfigId);
			AvatarPartType partType = (AvatarPartType)cfg.AvatarPartType;
			if (!AvatarEyePairUtility.HasBindPointForCollector(partsCollector, partType))
			{
				Log.Warning($"UILogin ChangeAvatar: ReferenceSpriteCollector 未绑定部位 key={partType}（眼睛需至少绑定 Eye_Front_Left/Right 或 Eye_Back_Left/Right 之一）");
				return;
			}

			string location = "Assets/Bundles/Avatar/" + cfg.Model;
			GameObject prefab = await resLoader.LoadAssetAsync<GameObject>(location);
			if (prefab == null)
			{
				Log.Error($"UILogin ChangeAvatar: 加载失败 location={location}");
				return;
			}

			SpriteRenderer prefabSr = prefab.GetComponent<SpriteRenderer>();
			if (prefabSr == null)
			{
				Log.Error($"UILogin ChangeAvatar: 预制体无 SpriteRenderer location={location}");
				return;
			}

			Sprite sprite = prefabSr.sprite;
			if (AvatarEyePairUtility.IsEyePairPart(partType))
			{
				AvatarEyePairUtility.ApplySpriteToEyePair(partsCollector, partType, sprite);
			}
			else
			{
				SpriteRenderer spriteRenderer = partsCollector.Get(partType.ToString());
				if (spriteRenderer == null)
				{
					return;
				}

				spriteRenderer.sprite = sprite;
			}
		}

		/// <summary>
		/// 根据 <paramref name="baseAvatar"/>（ConstantConfig 9013~9015 之一）收集模板里配置的全部 <see cref="AvatarConfig"/> Id（数量不限），
		/// 按配置顺序依次 <see cref="ChangeAvatar"/>；眼睛只配左眼 Id 时由 <see cref="AvatarEyePairUtility"/> 同步右眼。
		/// </summary>
		private static async ETTask ApplyBaseAvatarAsync(this UILoginComponent self, ReferenceSpriteCollector collector, int baseAvatar)
		{
			if (collector == null)
			{
				return;
			}

			List<int> partIds = new List<int>();
			DefaultAvatarHelper.CollectBaseAvatarDisplayConfigIds(baseAvatar, partIds);
			for (int i = 0; i < partIds.Count; i++)
			{
				await self.ChangeAvatar(partIds[i], collector);
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

			if (self.PendingCreateLeftBaseAvatar == 0)
			{
				self.PendingCreateLeftBaseAvatar = DefaultAvatarHelper.GetDefaultBaseAvatar();
			}

			LoginOperationResult result = await LoginHelper.LoginCreateRole(self.Root(), roleName, self.PendingCreateLeftBaseAvatar);
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

			if (self.PendingCreateRightBaseAvatar == 0)
			{
				self.PendingCreateRightBaseAvatar = DefaultAvatarHelper.GetDefaultBaseAvatar();
			}

			LoginOperationResult result = await LoginHelper.LoginCreateRole(self.Root(), roleName, self.PendingCreateRightBaseAvatar);
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
			LoginOperationResult result = await LoginHelper.LoginRoleEnterGame(self.Root(), roleId,self.PendingCreateLeftBaseAvatar);
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
			LoginOperationResult result = await LoginHelper.LoginRoleEnterGame(self.Root(), roleId,self.PendingCreateRightBaseAvatar);
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
			//显示模型
		}

		private static void SetLeftRole(this UILoginComponent self,RoleInfoProto roleInfoProto)
		{
			self.m_inputLeft.text = "";
			if (roleInfoProto==null)
			{
				self.m_btnLeftDelete.gameObject.SetActive(false);
				self.m_btnLeftTran.gameObject.SetActive(true);
				self.m_btnLeftCreate.gameObject.SetActive(true);
				self.m_btnLeftEnter.gameObject.SetActive(false);
				self.m_textLeftTitle.text = "角色1";
				self.m_inputLeft.gameObject.SetActive(true);
				
			}
			else
			{
				self.m_btnLeftDelete.gameObject.SetActive(true);
				self.m_btnLeftTran.gameObject.SetActive(false);
				self.m_btnLeftCreate.gameObject.SetActive(false);
				self.m_btnLeftEnter.gameObject.SetActive(true);
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
				self.m_btnRightTran.gameObject.SetActive(true);
				self.m_btnRightCreate.gameObject.SetActive(true);
				self.m_btnRightEnter.gameObject.SetActive(false);
				self.m_textRightTitle.text = "角色2";
				self.m_inputRight.gameObject.SetActive(true);
			}
			else
			{
				self.m_btnRightDelete.gameObject.SetActive(true);
				self.m_btnRightTran.gameObject.SetActive(false);
				self.m_btnRightCreate.gameObject.SetActive(false);
				self.m_btnRightEnter.gameObject.SetActive(true);
				self.m_inputRight.gameObject.SetActive(false);
				self.m_textRightTitle.text = roleInfoProto.Name;
			}
		}

		#endregion


	}
}
