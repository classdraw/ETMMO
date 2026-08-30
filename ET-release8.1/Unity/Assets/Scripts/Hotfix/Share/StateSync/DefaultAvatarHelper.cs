namespace ET
{
	/// <summary>
	/// 登录创角界面默认 UnitConfig Id 轮换。
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
	}
}
