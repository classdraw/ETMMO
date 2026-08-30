namespace ET.Client
{
	public static class LoginRoleDisplayHelper
	{
		public static string GetRaceName(int race)
		{
			switch (race)
			{
				case (int)FrameRoleRaceType.Human: return "人类";
				case (int)FrameRoleRaceType.Orc: return "兽人";
				case (int)FrameRoleRaceType.DarkElf: return "暗精灵";
				case (int)FrameRoleRaceType.Wolf: return "狼人";
				default: return $"种族{race}";
			}
		}

		public static string GetGenderName(int gender)
		{
			switch (gender)
			{
				case (int)FrameRoleGenderType.Male: return "男";
				case (int)FrameRoleGenderType.Female: return "女";
				default: return $"性别{gender}";
			}
		}
	}
}
