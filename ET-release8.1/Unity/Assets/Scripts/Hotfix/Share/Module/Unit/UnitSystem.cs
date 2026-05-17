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
        /// <summary>
        /// 是否是玩家
        /// </summary>
        /// <param name="self"></param>
        /// <returns></returns>
        public static bool IsPlayer(this Unit self)
        {
            return self.Type() == UnitType.Player;
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

        /// <summary>
        /// 是否是宠物
        /// </summary>
        /// <param name="self"></param>
        /// <returns></returns>
        public static bool IsPet(this Unit self)
        {
            return self.Type() == UnitType.Pet;
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