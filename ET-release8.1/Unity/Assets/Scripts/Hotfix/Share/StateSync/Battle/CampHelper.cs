namespace ET
{
    public static class CampHelper
    {
        public static bool IsAlly(Unit a, Unit b)
        {
            if (a.Id == b.Id) return true;
            a = ResolveOwner(a);
            b = ResolveOwner(b);
            if (a == null || b == null)
            {
                return true;//没有主人默认盟友
            }

            return GetFactionKey(a, a.MapId) == GetFactionKey(b, b.MapId);
        }

        public static bool IsHostile(Unit a, Unit b)
        {
            if (a.Id == b.Id) return false;
            a = ResolveOwner(a);
            b = ResolveOwner(b);
            if (a == null || b == null)
            {
                return false;//没有主人不是敌对
            }
            return GetFactionKey(a, a.MapId) != GetFactionKey(b, b.MapId);
        }

        public static FactionKey GetFactionKey(Unit unit, int mapConfigId)
        {
            if (GetMapType(mapConfigId) == MapType.FreePK && (unit.IsPlayer() || unit.IsRobot()))
            {
                return unit.TeamId > 0
                    ? new FactionKey(FactionKeyType.Team, unit.TeamId)
                    : new FactionKey(FactionKeyType.Player, unit.Id);
            }
            //|| unit.IsPet() || unit.IsSummon() 理论上这里unit肯定不是召唤物和宠物
            return unit.IsMonster()  ? CampConst.MonsterCamp : CampConst.PlayerCamp;
        }

        private static Unit ResolveOwner(Unit unit)
        {
            return ResolveOwner(unit, 0);
        }

        private static Unit ResolveOwner(Unit unit, int depth)
        {
            if (unit.OwnerId <= 0)
            {
                return unit;
            }

            if (depth > 8)
            {
                return null;//没有找到ownerUnit
            }

            Unit owner = unit.Scene().GetComponent<UnitComponent>().Get(unit.OwnerId);
            if (owner == null)
            {
                return null;//没有找到ownerUnit
            }

            return ResolveOwner(owner, depth + 1);
        }

        private static MapType GetMapType(int mapConfigId)
            => (MapType)MapConfigCategory.Instance.Get(mapConfigId).Type;
    }
}
