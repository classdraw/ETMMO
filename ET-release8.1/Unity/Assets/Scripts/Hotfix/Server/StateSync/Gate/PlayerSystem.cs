namespace ET.Server
{
    [EntitySystemOf(typeof(Player))]
    [FriendOf(typeof(Player))]
    [FriendOf(typeof(Unit))]
    public static partial class PlayerSystem
    {
        [EntitySystem]
        private static void Awake(this Player self, string accountName, string baseExternalDisplay, string name)
        {
            self.AccountName = accountName;
            self.BaseExternalDisplay = baseExternalDisplay;
            self.Name = name;
            self.SyncProfileFromExternalDisplay();
        }

        public static void SyncProfileFromExternalDisplay(this Player self)
        {
            ExternalDisplayConfigHelper.ResolveRoleProfile(
                self.BaseExternalDisplay, out int race, out int gender, out int configId);
            self.Race = race;
            self.Gender = gender;
            self.ConfigId = configId;
        }

        public static void ApplyProfileToUnit(this Player self, Unit unit)
        {
            unit.Race = self.Race;
            unit.Gender = self.Gender;
            unit.BaseExternalDisplay = self.BaseExternalDisplay ?? string.Empty;
        }

        /// <summary>
        /// 压测账号：Robot 配表 9101 + 默认外显（与 Player 分离）。
        /// </summary>
        public static void InitRobotProfile(this Player self)
        {
            self.BaseExternalDisplay = ExternalDisplayHelper.DefaultExternalDisplayVal;
            self.Race = ExternalDisplayHelper.DefaultRace;
            self.Gender = ExternalDisplayHelper.DefaultGender;
            self.ConfigId = RobotUnitHelper.RobotUnitConfigId;
        }

        public static UnitType GetGateUnitType(this Player self)
        {
            return RobotUnitHelper.IsRobotAccount(self.AccountName) ? UnitType.Robot : UnitType.Player;
        }

        public static int GetGateUnitConfigId(this Player self)
        {
            return self.GetGateUnitType() == UnitType.Robot ? RobotUnitHelper.RobotUnitConfigId : self.ConfigId;
        }
    }
}
