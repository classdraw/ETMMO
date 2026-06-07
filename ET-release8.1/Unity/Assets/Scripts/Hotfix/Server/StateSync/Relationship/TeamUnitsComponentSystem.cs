namespace ET.Server
{
    [EntitySystemOf(typeof(TeamUnitsComponent))]
    public static partial class TeamUnitsComponentSystem
    {
        [EntitySystem]
        private static void Awake(this TeamUnitsComponent self)
        {
        }

        [EntitySystem]
        private static void Destroy(this TeamUnitsComponent self)
        {
        }

        public static TeamUnit GetTeamUnit(this TeamUnitsComponent self, long teamId)
        {
            self.Children.TryGetValue(teamId, out Entity child);
            return child as TeamUnit;
        }

        public static TeamUnit GetOrCreateTeamUnit(this TeamUnitsComponent self, long teamId)
        {
            TeamUnit teamUnit = self.GetTeamUnit(teamId);
            if (teamUnit != null)
            {
                return teamUnit;
            }

            return self.AddChildWithId<TeamUnit>(teamId);
        }
        

        public static async ETTask<TeamComponent> LoadTeamComponent(this TeamUnitsComponent self, TeamUnit teamUnit, long teamId)
        {
            TeamComponent teamComponent = teamUnit.GetComponent<TeamComponent>();
            if (teamComponent != null)
            {
                return teamComponent;
            }

            if (teamId <= 0)
            {
                return null;
            }

            Scene root = self.Scene();
            teamComponent = await root.GetComponent<DBManagerComponent>().GetZoneDB(root.Zone()).Query<TeamComponent>(teamId);
            if (teamComponent != null)
            {
                teamUnit.AddComponent(teamComponent);
                return teamComponent;
            }

            return teamUnit.AddComponentWithId<TeamComponent>(teamId);
        }

        public static async ETTask SaveTeamComponent(this TeamUnit teamUnit)
        {
            TeamComponent teamComponent = teamUnit.GetComponent<TeamComponent>();
            if (teamComponent == null || teamComponent.Id <= 0)
            {
                return;
            }

            teamComponent.BeginInit();
            await teamUnit.Root().GetComponent<DBManagerComponent>().GetZoneDB(teamUnit.Zone()).Save(teamComponent);
        }
    }
}
