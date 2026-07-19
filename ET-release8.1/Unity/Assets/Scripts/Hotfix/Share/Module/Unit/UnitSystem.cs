using ET.Server;

namespace ET
{
    [EntitySystemOf(typeof(Unit))]
    public static partial class UnitSystem
    {
        [EntitySystem]
        private static void Awake(this Unit self, int configId,string name)
        {
            self.ConfigId = configId;
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
        //是否是战斗单位
        public static bool IsBattleUnit(this Unit self)
        {
            var tt = self.Type();
            if (tt==UnitType.Player||
                tt==UnitType.Monster||
                tt==UnitType.Pet||
                tt==UnitType.Summon||
                tt==UnitType.Robot||
                tt==UnitType.Bullet)
            {
                return true;
            }

            return false;
        }
        //是否战斗可以选择
        public static bool IsBattleSelect(this Unit self)
        {
            var tt = self.Type();
            if (tt==UnitType.Player||
                tt==UnitType.Monster||
                tt==UnitType.Pet||
                tt==UnitType.Summon||
                tt==UnitType.Robot)
            {
                return self.IsAlive();
            }

            return false;
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