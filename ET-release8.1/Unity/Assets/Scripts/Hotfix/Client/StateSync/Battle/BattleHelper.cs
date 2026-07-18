using Unity.Mathematics;

namespace ET.Client
{
    public static class BattleHelper
    {
        public static async ETTask GMCastSimple(Scene root,int castConfigId)
        {
            Unit monsterUnit = GetNearestMonsterUnit(root);
            Unit playerUnit=UnitHelper.GetMyUnitFromClientScene(root);
            if (playerUnit==null||playerUnit.IsDisposed)
            {
                Log.Console("主角玩家没有!!!");
                return;
            }

            if (monsterUnit==null||monsterUnit.IsDisposed)
            {
                Log.Console("附近没有怪物!!!");
                return;
            }

            if (CastConfigCategory.Instance.Get(castConfigId)==null)
            {
                Log.Console($"CastId {castConfigId} 不存在!!!");
                return;
            }

            C2M_GMTestCast c2MGmTestCast = C2M_GMTestCast.Create();
            c2MGmTestCast.CastConfigId = castConfigId;
            c2MGmTestCast.TargetId = monsterUnit.Id;
            c2MGmTestCast.InputPos = playerUnit.Position;
            M2C_GMTestCast m2CGmTestCast=await root.GetComponent<ClientSenderComponent>().Call(c2MGmTestCast) as M2C_GMTestCast;
            if (m2CGmTestCast.Error==ErrorCode.ERR_Success)
            {
                Log.Info("测试施法成功!!!");
            }
            else
            {
                 Log.Info($"测试施法失败 {m2CGmTestCast.Error}!!!");
                 EventSystem.Instance.Publish(root.CurrentScene(), new CastError(){CasterId = playerUnit.Id});
            }
        }

        /// <summary>
        /// 获取当前场景中距离自己最近的怪物 Unit。
        /// </summary>
        public static Unit GetNearestMonsterUnit(Scene root)
        {
            return GetNearestMonsterUnit(UnitHelper.GetMyUnitFromClientScene(root));
        }

        /// <summary>
        /// 获取距离指定单位最近的怪物 Unit。
        /// </summary>
        /// <param name="sourceUnit">参照单位，通常为玩家自身</param>
        /// <param name="maxRadiusMm">搜索半径（毫米），0 表示不限制</param>
        public static Unit GetNearestMonsterUnit(Unit sourceUnit, int maxRadiusMm = 0)
        {
            if (sourceUnit == null || sourceUnit.IsDisposed)
            {
                return null;
            }

            UnitComponent unitComponent = sourceUnit.Scene().GetComponent<UnitComponent>();
            if (unitComponent == null)
            {
                return null;
            }

            Unit nearest = null;
            float nearestDistSqr = float.MaxValue;
            float3 sourcePos = sourceUnit.Position;

            foreach (Entity entity in unitComponent.Children.Values)
            {
                if (entity is not Unit unit)
                {
                    continue;
                }

                if (unit.Id == sourceUnit.Id || !unit.IsMonster() || !unit.IsBattleSelect())
                {
                    continue;
                }

                if (!CampHelper.IsHostile(sourceUnit, unit))
                {
                    continue;
                }

                float dx = sourcePos.x - unit.Position.x;
                float dz = sourcePos.z - unit.Position.z;
                float distSqr = dx * dx + dz * dz;

                if (maxRadiusMm > 0)
                {
                    float radius = maxRadiusMm / 1000f;
                    if (distSqr > radius * radius)
                    {
                        continue;
                    }
                }

                if (distSqr < nearestDistSqr)
                {
                    nearestDistSqr = distSqr;
                    nearest = unit;
                }
            }

            return nearest;
        }
    }
}

