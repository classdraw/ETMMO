namespace ET
{
    [FriendOf(typeof(FactionComponent))]
    public static class CampHelper
    {
        public static bool IsAlly(Unit a, Unit b)
        {
            if (a.Id == b.Id) return true;
            if (!HasValidCampOwner(a) || !HasValidCampOwner(b))
            {
                return true;//没有主人默认盟友
            }

            return GetFactionId(a) == GetFactionId(b);
        }

        public static bool IsHostile(Unit a, Unit b)
        {
            if (a.Id == b.Id) return false;
            if (!HasValidCampOwner(a) || !HasValidCampOwner(b))
            {
                return false;//没有主人不是敌对
            }

            return GetFactionId(a) != GetFactionId(b);
        }

        /// <summary>
        /// 读取阵营 Id。宠物/召唤物始终与主人（Owner 链顶端）相同，与 MapType 无关。
        /// </summary>
        public static int GetFactionId(Unit unit)
        {
            if (TryGetOwnerFactionId(unit, out int ownerFactionId))
            {
                return ownerFactionId;
            }

            FactionComponent faction = unit.GetComponent<FactionComponent>();
            if (faction != null)
            {
                return faction.FactionId;
            }

            faction = unit.AddComponent<FactionComponent>();
            return faction.FactionId;
        }

        /// <summary>
        /// 按当前地图配置分配阵营（创建、切图、队伍变更时调用）。
        /// 宠物/召唤物不写地图规则，只同步主人 FactionId（全 MapType 生效）。
        /// </summary>
        public static void ApplyMapFaction(Unit unit, int mapConfigId)
        {
            if (unit == null || unit.IsDisposed)
            {
                return;
            }

            if (unit.IsPet() || unit.IsSummon())
            {
                Unit owner = ResolveCampOwner(unit);
                if (owner != null && owner.Id != unit.Id)
                {
                    ApplyMapFaction(owner, mapConfigId);
                    CopyFactionFromUnit(unit, owner);
                    return;
                }
            }

            int factionId = CalcFactionForMap(unit, mapConfigId);
            SetFactionId(unit, factionId);
            SyncFollowersFactionFromOwner(unit);
        }

        public static void CopyFactionFromUnit(Unit unit, Unit source)
        {
            SetFactionId(unit, GetFactionId(source));
        }

        private static void SetFactionId(Unit unit, int factionId)
        {
            FactionComponent faction = unit.GetComponent<FactionComponent>() ?? unit.AddComponent<FactionComponent>();
            faction.FactionId = factionId;
        }

        private static int CalcFactionForMap(Unit unit, int mapConfigId)
        {
            MapType mapType = GetMapType(mapConfigId);

            if (mapType == MapType.SafeZone)
            {
                return (int)CampType.CampA;
            }

            if (UsesWildMapFactionRules(unit))
            {
                return (int)CampType.CampB;
            }

            if (mapType == MapType.FreePK && IsPlayerSideBattleUnit(unit))
            {
                return unit.TeamId > 0
                    ? EncodeFreePkTeamFaction(unit.TeamId)
                    : EncodeFreePkPlayerFaction(unit.Id);
            }

            return (int)CampType.CampA;
        }

        private static bool IsPlayerSideBattleUnit(Unit unit)
        {
            return unit.IsPlayer();
        }

        /// <summary>野怪与地图 Robot（无玩家侧 Robot）共用怪物阵营规则。</summary>
        private static bool UsesWildMapFactionRules(Unit unit)
        {
            return unit.IsMonster() || unit.IsRobot();
        }

        /// <summary>自由 PK 队伍阵营，与静态 1/2 区分。</summary>
        private static int EncodeFreePkTeamFaction(long teamId)
        {
            int id = (int)(teamId & 0x7FFFFFFF);
            if (id == 0)
            {
                id = 1;
            }

            return -id;
        }

        /// <summary>自由 PK 单人阵营，与静态 1/2 区分。</summary>
        private static int EncodeFreePkPlayerFaction(long unitId)
        {
            int id = (int)(unitId & 0x7FFFFFFF);
            if (id <= (int)CampType.CampB)
            {
                id = (int)((unitId >> 32) & 0x7FFFFFFF);
            }

            if (id <= (int)CampType.CampB)
            {
                id = unchecked((int)(unitId ^ 0x40000000));
            }

            return id;
        }

        private static bool TryGetOwnerFactionId(Unit unit, out int factionId)
        {
            factionId = 0;
            if (!unit.IsPet() && !unit.IsSummon())
            {
                return false;
            }

            Unit owner = ResolveCampOwner(unit);
            if (owner == null || owner.Id == unit.Id)
            {
                return false;
            }

            factionId = GetFactionId(owner);
            return true;
        }

        /// <summary>主人切图/组队后，刷新其宠物、召唤物组件上的 FactionId。</summary>
        private static void SyncFollowersFactionFromOwner(Unit owner)
        {
            UnitComponent unitComponent = owner.Scene()?.GetComponent<UnitComponent>();
            if (unitComponent == null)
            {
                return;
            }

            foreach (Entity entity in unitComponent.Children.Values)
            {
                if (entity is not Unit follower || follower.IsDisposed)
                {
                    continue;
                }

                if (!follower.IsPet() && !follower.IsSummon())
                {
                    continue;
                }

                Unit campOwner = ResolveCampOwner(follower);
                if (campOwner != null && campOwner.Id == owner.Id)
                {
                    CopyFactionFromUnit(follower, owner);
                }
            }
        }

        private static bool HasValidCampOwner(Unit unit)
        {
            if (unit.OwnerId <= 0)
            {
                return true;
            }

            return ResolveCampOwner(unit) != null;
        }

        /// <summary>OwnerId 链顶端的战斗主人（宠物/召唤物用于跟阵营）。</summary>
        private static Unit ResolveCampOwner(Unit unit)
        {
            return ResolveCampOwner(unit, 0);
        }

        private static Unit ResolveCampOwner(Unit unit, int depth)
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

            return ResolveCampOwner(owner, depth + 1);
        }

        private static MapType GetMapType(int mapConfigId)
            => (MapType)MapConfigCategory.Instance.Get(mapConfigId).Type;
    }
}
