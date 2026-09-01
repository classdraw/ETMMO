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
    }
}
