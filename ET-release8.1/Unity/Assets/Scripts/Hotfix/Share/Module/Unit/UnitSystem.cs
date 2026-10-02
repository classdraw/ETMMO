namespace ET
{
    [EntitySystemOf(typeof(Unit))]
    public static partial class UnitSystem
    {
        [EntitySystem]
        private static void Awake(this Unit self, int configId,string name)
        {
            self.ConfigId = configId;
            self.TableConfigId = 0;
            self.Name = name;
        }

        public static UnitConfig Config(this Unit self)
        {
            return UnitConfigCategory.Instance.Get(self.ConfigId);
        }

        public static UnitType Type(this Unit self)
        {
            return (UnitType)self.Config().Type;
        }
        /// <summary>
        /// 可被技能/形状选中的战斗实体类型（玩家、怪、宠、召唤、机器人；不含子弹、NPC）。
        /// </summary>
        public static bool IsBattleSelectableType(this Unit self)
        {
            UnitType tt = self.Type();
            return tt == UnitType.Player
                || tt == UnitType.Monster
                || tt == UnitType.Pet
                || tt == UnitType.Summon
                || tt == UnitType.Robot;
        }

        /// <summary>
        /// 是否参与战斗逻辑的单位（含子弹；不判断存活）。
        /// </summary>
        public static bool IsBattleUnit(this Unit self)
        {
            return self.IsBattleSelectableType() || self.IsBullet();
        }

        /// <summary>
        /// 是否是玩家
        /// </summary>
        /// <param name="self"></param>
        /// <returns></returns>
        public static bool IsPlayer(this Unit self)
        {
            return self.Type() == UnitType.Player;
        }
        
        public static bool IsRobot(this Unit self)
        {
            return self.Type() == UnitType.Robot;
        }

        /// <summary>
        /// 需要同步 M2C 视野与移动消息的真实客户端控制单位（玩家或压测机器人）。
        /// </summary>
        public static bool IsClientAvatar(this Unit self)
        {
            return self.IsPlayer() || self.IsRobot();
        }
        /// <summary>
        /// 是否是怪物
        /// </summary>
        /// <param name="self"></param>
        /// <returns></returns>
        public static bool IsMonster(this Unit self)
        {
            return self.Type() == UnitType.Monster;
        }
        /// <summary>
        /// 是否是npc
        /// </summary>
        /// <param name="self"></param>
        /// <returns></returns>
        public static bool IsNpc(this Unit self)
        {
            return self.Type() == UnitType.NPC;
        }
        public static bool IsBullet(this Unit self)
        {
            return self.Type() == UnitType.Bullet;
        }
        /// <summary>
        /// 是否是宠物
        /// </summary>
        /// <param name="self"></param>
        /// <returns></returns>
        public static bool IsPet(this Unit self)
        {
            return self.Type() == UnitType.Pet;
        }

        /// <summary>
        /// 是否是召唤物
        /// </summary>
        public static bool IsSummon(this Unit self)
        {
            return self.Type() == UnitType.Summon;
        }
        
        [EntitySystem]
        private static void GetComponentSys(this ET.Unit unit, System.Type type)
        {
            if (typeof(IUnitCache).IsAssignableFrom(type)|| typeof(ITransfer).IsAssignableFrom(type))
            {
                EventSystem.Instance.Publish(unit.Scene(),new UnitGetComponent{Type = type,Unit = unit});
            }
        }
    }
}