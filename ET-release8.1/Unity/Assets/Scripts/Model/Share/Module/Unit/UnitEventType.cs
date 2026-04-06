using Unity.Mathematics;

namespace ET
{
    
    /// <summary>
    /// 玩家进入游戏(可以推送消息)
    /// </summary>
    public struct UnitEnterGame
    {
        public Unit Unit;
    }
    public struct ChangePosition
    {
        public Unit Unit;
        public float3 OldPos;
    }

    public struct ChangeRotation
    {
        public Unit Unit;
    }
    
    /// <summary>
    /// 检测玩家身上装备等配置表
    /// </summary>
    public struct UnitCheckCfg
    {
        public Unit Unit;
    }
    
    /// <summary>
    /// 重新计算玩家属性(上线后调用)
    /// </summary>
    public struct UnitReEffect
    {
        public Unit Unit;
    }

    /// <summary>
    /// 地图角色下线：在移除 AOI 之后、回收 Unit 之前由地图服发布，订阅方可写库或同步持久化数据。
    /// </summary>
    public struct UnitOfflinePersist
    {
        public Unit Unit;
    }
}