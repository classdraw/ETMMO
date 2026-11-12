using Unity.Mathematics;

namespace ET
{
    /// <summary>怪物巡逻状态（出生点半径随机点），与机器人用 XunLuoPathComponent 分离。</summary>
    [ComponentOf(typeof(Unit))]
    public class MonsterPatrolComponent : Entity, IAwake
    {
        public float3 SpawnCenter;

        /// <summary>A/1000：巡逻半径。</summary>
        public float PatrolRadiusM;

        /// <summary>B/1000：巡逻间隔（毫秒）。</summary>
        public int PatrolIntervalMs;

        public long NextMoveTime;

        /// <summary>0 待机；1 已下发寻路，等待到达。</summary>
        public byte PatrolPhase;

        /// <summary>C=1 时追击玩家。</summary>
        public bool ChaseEnabled;

        /// <summary>D/1000：追击范围（相对出生点），0 不限制（仅 C=1）。</summary>
        public float ChaseRangeM;

        /// <summary>E/1000：追击间隔/休息时间（毫秒，仅 C=1）。</summary>
        public int ChaseRestMs;

        /// <summary>F/1000：追击时与玩家最近停靠距离（仅 C=1）。</summary>
        public float ChaseMinDistanceM;

        /// <summary>0 巡逻；1 追击；2 返回出生点。</summary>
        public byte MoveIntent;

        public long ChaseTargetId;
    }
}
