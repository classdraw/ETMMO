namespace ET.Client
{
	public static class LoginRoleDisplayHelper
	{
		public static string GetRaceName(int race)
		{
			return RaceConfigHelper.GetRaceDisplayName(race);
		}

		public static string GetJobName(int job)
		{
			if (job <= 0)
			{
				return string.Empty;
			}

			return JobConfigHelper.GetJobDisplayName(job);
		}
	}
}
