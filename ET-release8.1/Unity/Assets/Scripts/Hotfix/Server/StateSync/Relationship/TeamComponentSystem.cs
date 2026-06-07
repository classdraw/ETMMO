using System;
using System.Collections.Generic;

namespace ET.Server
{
    [EntitySystemOf(typeof(TeamComponent))]
    [FriendOf(typeof(TeamComponent))]
    [FriendOf(typeof(TeamUnit))]
    [FriendOf(typeof(TeamUnitsComponent))]
    public static partial class TeamComponentSystem
    {
        private const long NoTeamContainerId = 0;

        [EntitySystem]
        private static void Awake(this TeamComponent self)
        {
        }

        [EntitySystem]
        private static void Destroy(this TeamComponent self)
        {
            self.MemberUnitIds.Clear();
            self.OnlineUnitIds.Clear();
        }
        
        [EntitySystem]
        private static void Deserialize(this ET.Server.TeamComponent self)
        {

        }

        public static void SetOnline(this TeamComponent self, long unitId)
        {
            if (unitId > 0&&!self.OnlineUnitIds.Contains(unitId))
            {
                self.OnlineUnitIds.Add(unitId);
            }
        }

        public static void SetOffline(this TeamComponent self, long unitId)
        {
            self.OnlineUnitIds.Remove(unitId);
        }


        public static async ETTask RemoveTeamFromDb(this TeamUnitsComponent self, long teamId)
        {
            await self.Scene().GetComponent<DBManagerComponent>().GetZoneDB(self.Zone()).Remove<TeamComponent>(teamId);
        }
    }
}
