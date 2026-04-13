using System;
using System.Collections.Generic;

namespace ET
{
	/// <summary>
	/// 基底外观：角色只存 <see cref="RoleInfoProto.BaseAvatar"/>（9013/9014/9015 常量表 Id）。
	/// 模板行 <c>StringValue</c> 为任意多个 <see cref="AvatarConfig"/> Id（逗号/分号/竖线分隔），按书写顺序依次换装，配置几项就应用几项。
	/// 眼睛请在表里只配左眼对应的 AvatarConfig Id（前眼/后眼各至多一项），显示层会同步到右眼。
	/// 若模板行不存在、为空或解析不到任何有效 Id，则列表为空（不再从其它常量行自动补齐）。
	/// </summary>
	public static class DefaultAvatarHelper
	{
		public const int ConstantBaseAvatarA = 9013;
		public const int ConstantBaseAvatarB = 9014;
		public const int ConstantBaseAvatarC = 9015;

		private static readonly int[] BaseAvatarPool = { ConstantBaseAvatarA, ConstantBaseAvatarB, ConstantBaseAvatarC };

		public static int GetDefaultBaseAvatar()
		{
			return ConstantBaseAvatarA;
		}

		public static int RollRandomBaseAvatar()
		{
			int i = RandomGenerator.RandomNumber(0, BaseAvatarPool.Length);
			return BaseAvatarPool[i];
		}

		/// <summary>
		/// 顺序轮换基底外观：在 <see cref="BaseAvatarPool"/> 内按 index++，超过长度则回到 0。
		/// 若 <paramref name="currentBaseAvatar"/> 不在池内，则返回池的第 0 项。
		/// </summary>
		public static int NextBaseAvatar(int currentBaseAvatar)
		{
			if (BaseAvatarPool == null || BaseAvatarPool.Length == 0)
			{
				return 0;
			}

			int idx = -1;
			for (int i = 0; i < BaseAvatarPool.Length; i++)
			{
				if (BaseAvatarPool[i] == currentBaseAvatar)
				{
					idx = i;
					break;
				}
			}

			int nextIndex = idx + 1;
			if (nextIndex < 0 || nextIndex >= BaseAvatarPool.Length)
			{
				nextIndex = 0;
			}

			return BaseAvatarPool[nextIndex];
		}

		public static int GetBaseAvatarFromRoleOrDefault(RoleInfoProto roleOrNull)
		{
			if (roleOrNull == null || roleOrNull.BaseAvatar == 0)
			{
				return RollRandomBaseAvatar();
			}

			return roleOrNull.BaseAvatar;
		}

		/// <summary>
		/// 收集要在界面上按序应用的 AvatarConfig Id：仅来自基底模板行（9013 等）的 StringValue 列表。
		/// </summary>
		public static void CollectBaseAvatarDisplayConfigIds(int baseAvatarConstantId, List<int> dest)
		{
			if (dest == null)
			{
				throw new ArgumentNullException(nameof(dest));
			}

			dest.Clear();

			if (baseAvatarConstantId != 0
			    && ConstantConfigCategory.Instance.Contain(baseAvatarConstantId))
			{
				string raw = ConstantConfigCategory.Instance.Get(baseAvatarConstantId).StringValue;
				if (!string.IsNullOrWhiteSpace(raw))
				{
					string[] parts = raw.Split(new[] { ',', ';', '|' }, StringSplitOptions.RemoveEmptyEntries);
					for (int i = 0; i < parts.Length; i++)
					{
						if (int.TryParse(parts[i].Trim(), out int id) && id != 0)
						{
							dest.Add(id);
						}
					}
				}
			}
		}
	}
}
