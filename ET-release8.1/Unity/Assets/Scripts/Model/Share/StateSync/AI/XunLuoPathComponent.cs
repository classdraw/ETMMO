using Unity.Mathematics;

namespace ET
{
    [ComponentOf(typeof(Unit))]
    public class XunLuoPathComponent : Entity, IAwake
    {
        public float3[] path;
        public int Index;
        public long NextMoveTime;

        /// <summary>0 待机/可发下一格；1 已下发寻路，等待 MoveComponent 到达。</summary>
        public byte PatrolPhase;
    }
}
