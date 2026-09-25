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

            if (playerUnit.IsForbidSkill())
            {
                Log.Console("禁止施法状态!!!");
                EventSystem.Instance.Publish(root.CurrentScene(), new CastError(){CasterId = playerUnit.Id});
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
        /// 朝角色真前方指定距离的坐标点释放技能（不选单位）。
        /// </summary>
        public static async ETTask GMCastForward(Scene root, int castConfigId, float distance = 5f)
        {
            Unit playerUnit = UnitHelper.GetMyUnitFromClientScene(root);
            if (playerUnit == null || playerUnit.IsDisposed)
            {
                Log.Console("主角玩家没有!!!");
                return;
            }

            if (playerUnit.IsForbidSkill())
            {
                Log.Console("禁止施法状态!!!");
                EventSystem.Instance.Publish(root.CurrentScene(), new CastError() { CasterId = playerUnit.Id });
                return;
            }

            if (!CastConfigCategory.Instance.Contain(castConfigId))
            {
                Log.Console($"CastId {castConfigId} 不存在!!!");
                return;
            }

            float3 inputPos = playerUnit.Position + GetCardinalForward(playerUnit.Forward) * distance;
            inputPos.y = playerUnit.Position.y;

            C2M_GMTestCast c2MGmTestCast = C2M_GMTestCast.Create();
            c2MGmTestCast.CastConfigId = castConfigId;
            c2MGmTestCast.TargetId = 0;
            c2MGmTestCast.InputPos = inputPos;
            M2C_GMTestCast m2CGmTestCast = await root.GetComponent<ClientSenderComponent>().Call(c2MGmTestCast) as M2C_GMTestCast;
            if (m2CGmTestCast.Error == ErrorCode.ERR_Success)
            {
                Log.Info("测试施法成功!!!");
            }
            else
            {
                Log.Info($"测试施法失败 {m2CGmTestCast.Error}!!!");
                EventSystem.Instance.Publish(root.CurrentScene(), new CastError() { CasterId = playerUnit.Id });
            }
        }

        /// <summary>
        /// 将朝向收成四方向：右(+x) / 左(-x) / 上(+z) / 下(-z)，与 Animator2D 面向一致。
        /// </summary>
        private static float3 GetCardinalForward(float3 forward)
        {
            forward.y = 0;
            if (math.lengthsq(forward) <= math.EPSILON)
            {
                return new float3(0f, 0f, -1f);
            }

            forward = math.normalize(forward);
            if (math.abs(forward.x) > math.abs(forward.z))
            {
                return forward.x >= 0f ? new float3(1f, 0f, 0f) : new float3(-1f, 0f, 0f);
            }

            return forward.z >= 0f ? new float3(0f, 0f, 1f) : new float3(0f, 0f, -1f);
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

                if (unit.Id == sourceUnit.Id || !unit.IsMonster() || !IsAliveUnit(unit))
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

        public static bool IsAliveUnit(Unit unit)
        {
            NumericComponent numericComponent = unit.GetComponent<NumericComponent>();
            if (numericComponent == null)
            {
                return true;
            }

            return numericComponent[NumericType.Hp] > 0;
        }
    }
}

