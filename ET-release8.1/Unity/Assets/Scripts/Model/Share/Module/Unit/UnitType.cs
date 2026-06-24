namespace ET
{
    public enum UnitType: byte
    {
        Player = 1,//b
        Monster = 2,//b
        NPC = 3,
        Pet=4,//宠物b
        Collect=5,//采集物
        Drop=6,//掉落物
        Mount=7,//坐骑
        Summon=8,//召唤物b
        Robot=9,//机器人b
        Env=10,//环境生物
        Bullet=11,//子弹
    }
    
    /// <summary>
    /// 角色子类型定义
    /// </summary>
    public enum UnitSubType
    {
        None = 0,

        Boss = 21,

        /// <summary>
        /// 精英
        /// </summary>
        Elite = 22,

        /// <summary>
        /// 小怪
        /// </summary>
        Monster = 23,
    }
}