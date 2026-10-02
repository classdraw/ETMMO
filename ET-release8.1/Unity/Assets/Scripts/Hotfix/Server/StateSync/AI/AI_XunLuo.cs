using Unity.Mathematics;

namespace ET.Server
{
    /// <summary>地图 Unit 巡逻（怪物 / 压测 Robot）。Name 须与 AIConfig.Name 一致。</summary>
    [FriendOf(typeof(XunLuoPathComponent))]
    public class AI_XunLuo : AAIHandler
    {
        public override int Check(AIComponent aiComponent, AIConfig aiConfig)
        {
            Unit unit = aiComponent.GetParent<Unit>();
            if (unit == null)
            {
                return 1;
            }

            XunLuoPathComponent path = unit.GetComponent<XunLuoPathComponent>();
            if (path == null || path.path == null || path.path.Length == 0)
            {
                return 1;
            }

            if (unit.IsCasting() || unit.IsForbidMove())
            {
                return 1;
            }

            return 0;
        }

        public override int Update(AIComponent aiComponent, AIConfig aiConfig)
        {
            Unit unit = aiComponent.GetParent<Unit>();
            if (unit == null)
            {
                return 1;
            }

            XunLuoPathComponent path = unit.GetComponent<XunLuoPathComponent>();
            if (path?.path == null || path.path.Length == 0)
            {
                return 1;
            }

            MoveComponent move = unit.GetComponent<MoveComponent>();

            if (unit.IsCasting() || unit.IsForbidMove())
            {
                if (move != null && !move.IsArrived())
                {
                    move.Stop(false);
                }

                path.PatrolPhase = 0;
                return 0;
            }

            if (path.PatrolPhase == 1)
            {
                if (move != null && !move.IsArrived())
                {
                    return 0;
                }

                path.MoveNext();
                path.PatrolPhase = 0;
            }

            if (path.PatrolPhase != 0)
            {
                return 0;
            }

            if (TimeInfo.Instance.ServerNow() < path.NextMoveTime)
            {
                return 0;
            }

            path.NextMoveTime = TimeInfo.Instance.ServerNow() + RandomGenerator.RandomNumber(8 * 1000, 15 * 1000);
            float3 target = path.GetCurrent();
            unit.FindPathMoveToAsync(target).Coroutine();
            path.PatrolPhase = 1;
            return 0;
        }
    }
}
