using UnityEngine;

namespace ET.Client
{
	[EntitySystemOf(typeof(Avatar2DComponent))]
	[FriendOf(typeof(Avatar2DComponent))]
	public static partial class Avatar2DComponentSystem
	{
		[EntitySystem]
		private static void Awake(this Avatar2DComponent self)
		{
			GameObjectComponent gameObjectComponent = self.GetParent<Unit>().GetComponent<GameObjectComponent>();
			if (gameObjectComponent?.GameObject == null)
			{
				return;
			}

			Unit unit = self.GetParent<Unit>();
			self.AnimPlayer = gameObjectComponent.GameObject.GetComponentInChildren<FrameSheetAnimPlayer>();
			if (self.AnimPlayer == null)
			{
				return;
			}

			// 仅怪物五段外显拼装时驱动 RedressAvatar；玩家创角档案为 Race,Gender,Job 三段。
			if (ShouldUsePartAssemblyRedress(unit.BaseExternalDisplay))
			{
				self.RedressAvatar = self.AnimPlayer.GetComponent<RedressAvatar>();
				if (self.RedressAvatar == null)
				{
					self.RedressAvatar = self.AnimPlayer.gameObject.AddComponent<RedressAvatar>();
				}

				self.Refresh();
			}
		}

		[EntitySystem]
		private static void Destroy(this Avatar2DComponent self)
		{
			self.AnimPlayer = null;
			self.RedressAvatar = null;
		}

		public static void Refresh(this Avatar2DComponent self)
		{
			Unit unit = self.GetParent<Unit>();
			if (unit == null || unit.IsDisposed)
			{
				return;
			}

			string baseExternalDisplay = unit.BaseExternalDisplay;
			if (string.IsNullOrEmpty(baseExternalDisplay))
			{
				return;
			}

			if (!ShouldUsePartAssemblyRedress(baseExternalDisplay)
			    || !ExternalDisplayHelper.TryParseExternalDisplayString(baseExternalDisplay, out ExternalDisplayAppearance appearance))
			{
				return;
			}

			RoleTextureComponent roleTex = self.Root().GetComponent<RoleTextureComponent>();
			if (roleTex == null || self.AnimPlayer == null)
			{
				return;
			}

			roleTex.ValidateAppearance(ref appearance);
			UILoginRolePreviewHelper.ApplyPreview(self.AnimPlayer, roleTex, appearance);
		}

		private static bool ShouldUsePartAssemblyRedress(string baseExternalDisplay)
		{
			if (string.IsNullOrEmpty(baseExternalDisplay))
			{
				return false;
			}

			if (RoleCreateProfileHelper.TryParseProfileString(baseExternalDisplay, out _))
			{
				return false;
			}

			return ExternalDisplayHelper.TryParseExternalDisplayString(baseExternalDisplay, out _);
		}
	}
}
