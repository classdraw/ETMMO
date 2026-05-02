using System;
using System.Collections.Generic;

namespace ET
{
	/// <summary>
	/// 基底外观：角色可存常量表 Id（9013/9014/9015）用于 <see cref="CollectBaseAvatarDisplayConfigIds"/>。
	/// 模板行 <c>StringValue</c> 为任意多个 <see cref="AvatarConfig"/> Id（逗号/分号/竖线分隔），按书写顺序依次换装，配置几项就应用几项。
	/// 眼睛请在表里只配左眼对应的 AvatarConfig Id（前眼/后眼各至多一项），显示层会同步到右眼。
	/// 若模板行不存在、为空或解析不到任何有效 Id，则列表为空（不再从其它常量行自动补齐）。
	/// </summary>
	public static class DefaultAvatarHelper
	{

		/// <summary>登录创角界面轮换池长度（仅 const，满足 Hotfix 程序集约束）。</summary>
		public const int DefaultRoleUnitConfigIdCount = 5;

		public const int DefaultRoleUnitConfigId0 = 1001;
		public const int DefaultRoleUnitConfigId1 = 1002;
		public const int DefaultRoleUnitConfigId2 = 1003;
		public const int DefaultRoleUnitConfigId3 = 1004;
		public const int DefaultRoleUnitConfigId4 = 1005;

		public static int GetDefaultRoleUnitConfigId()
		{
			return DefaultRoleUnitConfigId0;
		}

		/// <summary>在 1001~1005 固定池内顺序轮换；<paramref name="currentConfigId"/> 为 0 或不在池内时回到首项。</summary>
		public static int NextRoleUnitConfigId(int currentConfigId)
		{
			if (currentConfigId == 0)
			{
				return DefaultRoleUnitConfigId0;
			}

			int idx = IndexOfRoleUnitConfigId(currentConfigId);
			if (idx < 0)
			{
				return DefaultRoleUnitConfigId0;
			}

			return GetRoleUnitConfigIdByIndex(idx + 1);
		}

		public static int RollRandomRoleUnitConfigId()
		{
			int offset = RandomGenerator.RandomNumber(0, DefaultRoleUnitConfigIdCount);
			return GetRoleUnitConfigIdByIndex(offset);
		}

		private static int IndexOfRoleUnitConfigId(int configId)
		{
			switch (configId)
			{
				case DefaultRoleUnitConfigId0: return 0;
				case DefaultRoleUnitConfigId1: return 1;
				case DefaultRoleUnitConfigId2: return 2;
				case DefaultRoleUnitConfigId3: return 3;
				case DefaultRoleUnitConfigId4: return 4;
				default: return -1;
			}
		}

		private static int GetRoleUnitConfigIdByIndex(int index)
		{
			switch (index % DefaultRoleUnitConfigIdCount)
			{
				case 0: return DefaultRoleUnitConfigId0;
				case 1: return DefaultRoleUnitConfigId1;
				case 2: return DefaultRoleUnitConfigId2;
				case 3: return DefaultRoleUnitConfigId3;
				default: return DefaultRoleUnitConfigId4;
			}
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
