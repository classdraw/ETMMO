using Unity.Mathematics;

namespace ET.Server
{
    [EntitySystemOf(typeof(Cast))]
    [FriendOf(typeof(Cast))]
    public static partial class CastSystem
    {
        [EntitySystem]
        private static void Awake(this ET.Server.Cast self,int configId)
        {
            self.ConfigId = configId;
        }
        [EntitySystem]
        private static void Destroy(this ET.Server.Cast self)
        {
            self.ConfigId = 0;
            self.Caster = null;
            self.Targets.Clear();
            self.InputUnitId = 0;
            self.InputPos = default;
            self.StartTime = 0;
            self.InputUnit = null;
        }

        /// <summary>
        /// 释放技能
        /// </summary>
        /// <param name="cast"></param>
        /// <returns></returns>
        public static int Cast(this Cast cast)
        {
            int err = cast.CastCheck();//释放条件判断
            if (err!=ErrorCode.ERR_Success)
            {
                return err;
            }
            cast.SelectTargets();//选择目标
            err = cast.CastCheckBeforeBegin();//开始释放条件判断
            if (err!=ErrorCode.ERR_Success)
            {
                return err;
            }
            
            cast.CastBeginAsync().Coroutine();//释放
            return ErrorCode.ERR_Success;
        }

        /// <summary>
        /// 释放技能前置判断
        /// </summary>
        /// <param name="cast"></param>
        /// <returns></returns>
        public static int CastCheck(this Cast cast)
        {
            if (cast==null||cast.IsDisposed)
            {
                return ErrorCode.ERR_CastArgsError;
            }

            Unit caster = cast.Caster;
            if (caster==null||caster.IsDisposed)
            {
                return ErrorCode.ERR_CastCasterIsNullError;
            }
            
            SelectType selectType = (SelectType)cast.Config.SelectType;
            if (selectType==SelectType.Self)//选自己肯定没问题
            {
                return ErrorCode.ERR_Success;
            }
            
            if (selectType==SelectType.Position)
            {
                ShapeType shapeType =(ShapeType)cast.Config.SelectParam[0];
                if (shapeType==ShapeType.Single)//选的是坐标 必须有选择器
                {
                    return ErrorCode.ERR_CastConfigError;
                }
            }
            
            Unit inputUnit = caster.Scene().GetComponent<UnitComponent>().Get(cast.InputUnitId);
            if (inputUnit == null || inputUnit.IsDisposed)
            {
                return ErrorCode.ERR_CastInputUnitError;
            }
            
            switch (selectType)
            {
                //需要一个目标，那么前端需要给一个目标
                case SelectType.FriendlyTarget:
                {
                    if (CampHelper.IsHostile(caster,inputUnit))
                    {
                        //阵营不对
                        return ErrorCode.ERR_CastInputUnitError;
                    }
                    break;
                }
                case SelectType.EnemyTarget:
                {
                    if (CampHelper.IsAlly(caster,inputUnit))
                    {
                        //阵营不对
                        return ErrorCode.ERR_CastInputUnitError;
                    }
                    break;
                }
            }

            cast.InputUnit = inputUnit;
            return ErrorCode.ERR_Success;
        }

        /// <summary>
        /// 释放前检测
        /// </summary>
        /// <param name="cast"></param>
        /// <returns></returns>
        public static int CastCheckBeforeBegin(this Cast cast)
        {
            return ErrorCode.ERR_Success;
        }
        
        /// <summary>
        /// 技能释放
        /// </summary>
        /// <param name="cast"></param>
        public static async ETTask CastBeginAsync(this Cast cast)
        {
            await ETTask.CompletedTask;
        }

        
        /// <summary>
        /// 选择目标
        /// </summary>
        /// <param name="cast"></param>
        public static void SelectTargets(this Cast cast)
        {
            Unit caster = cast.Caster;
            cast.Targets.Clear();
            SelectType selectType = (SelectType)cast.Config.SelectType;
            switch (selectType)
            {
                //不处理none，属于异常
                case SelectType.Self:
                    cast.SelectTargetsInner(cast.InputUnit,float3.zero);
                    break;
                case SelectType.FriendlyTarget:
                case SelectType.EnemyTarget:
                    cast.SelectTargetsInner(cast.InputUnit,float3.zero);
                    break;
                case SelectType.Position:
                    cast.SelectTargetsInner(null,cast.InputPos);
                    break;
                default:
                    Log.Error($"未知目标选择类型: {selectType}");
                    break;
            }
        }
        

        private static void SelectTargetsInner(this Cast cast,Unit unit,float3 pos)
        {
            Unit caster = cast.Caster;
            ShapeType shapeType =(ShapeType)cast.Config.SelectParam[0];
            if (shapeType==ShapeType.Single)
            {
                if (unit!=null)
                {
                    if (IsMatchSelectCampType(caster, unit, GetSingleSelectCampType(cast)))
                    {
                        cast.Targets.Add(unit.Id);
                    }
                }
                else
                {
                    //选一个坐标 就是没有目标
                }
                return;
            }

            if (unit!=null)
            {
                pos = unit.Position;//根据这个坐标进行筛选
            }

            switch (shapeType)
            {
                case ShapeType.Circle://圆形
                {
                    
                    float radius = cast.Config.SelectParam[1] / 1000f;
                    float radiusSqr = radius * radius; 
                    int needCount = cast.Config.SelectParam[2];
                    SelectCampType selectCampType = (SelectCampType)cast.Config.SelectParam[3];
                    int nowCount = 0;
                    foreach (AOIEntity aoiEntity in caster.GetBeSeePlayers().Values)
                    {
                        Unit targetUnit = aoiEntity.GetParent<Unit>();
                        if (targetUnit==caster)
                        {//不需要是自己，如果一定要自己，自己配置self和single就可以了
                            continue;
                        }

                        float disSqr = math.lengthsq(pos-targetUnit.Position);
                        if (disSqr<=radiusSqr && IsMatchSelectCampType(caster, targetUnit, selectCampType))
                        {
                            cast.Targets.Add(targetUnit.Id);
                            nowCount++;
                        }

                        if (nowCount>=needCount)
                        {
                            break;
                        }
                    }
                    break;
                }
                case ShapeType.Rectangle://矩形
                {
                    float length = cast.Config.SelectParam[1] / 1000f;
                    float height = cast.Config.SelectParam[2] / 1000f;
                    int val = cast.Config.SelectParam[3];
                    int needCount = cast.Config.SelectParam[4];
                    SelectCampType selectCampType = (SelectCampType)cast.Config.SelectParam[5];
                    int nowCount = 0;

                    float halfLength = length * 0.5f;
                    float halfHeight = height * 0.5f;
                    const float minDirectionSqr = 0.01f;

                    bool axisAligned = val == 0;
                    float3 forward = new float3(0, 0, 1);
                    float3 right = new float3(1, 0, 0);

                    if (val == 1)
                    {
                        float3 direction = pos - caster.Position;
                        direction.y = 0;
                        if (math.lengthsq(direction) > minDirectionSqr)
                        {
                            forward = math.normalize(direction);
                            right = math.normalize(new float3(forward.z, 0, -forward.x));
                        }
                        else
                        {
                            axisAligned = true;
                        }
                    }

                    foreach (AOIEntity aoiEntity in caster.GetBeSeePlayers().Values)
                    {
                        Unit targetUnit = aoiEntity.GetParent<Unit>();
                        if (targetUnit == caster)
                        {
                            continue;
                        }

                        float3 offset = targetUnit.Position - pos;
                        offset.y = 0;

                        bool inside;
                        if (axisAligned)
                        {
                            inside = math.abs(offset.x) <= halfLength && math.abs(offset.z) <= halfHeight;
                        }
                        else
                        {
                            float localLength = math.dot(offset, forward);
                            float localHeight = math.dot(offset, right);
                            inside = math.abs(localLength) <= halfLength && math.abs(localHeight) <= halfHeight;
                        }

                        if (inside && IsMatchSelectCampType(caster, targetUnit, selectCampType))
                        {
                            cast.Targets.Add(targetUnit.Id);
                            nowCount++;
                        }

                        if (nowCount >= needCount)
                        {
                            break;
                        }
                    }

                    break;
                }
                case ShapeType.Fan://扇形
                {
                    int val = cast.Config.SelectParam[1];
                    float angle = cast.Config.SelectParam[2] / 1000f;
                    int needCount = cast.Config.SelectParam[3];
                    SelectCampType selectCampType = (SelectCampType)cast.Config.SelectParam[4];
                    int nowCount = 0;
                    const float minDirectionSqr = 0.01f;
                    float halfAngle = angle * 0.5f;

                    float3 forward;
                    if (val == 1)
                    {
                        float3 direction = pos - caster.Position;
                        direction.y = 0;
                        if (math.lengthsq(direction) > minDirectionSqr)
                        {
                            forward = math.normalize(direction);
                        }
                        else
                        {
                            forward = caster.Forward;
                            forward.y = 0;
                            forward = math.lengthsq(forward) > minDirectionSqr ? math.normalize(forward) : new float3(0, 0, 1);
                        }
                    }
                    else
                    {
                        forward = caster.Forward;
                        forward.y = 0;
                        forward = math.lengthsq(forward) > minDirectionSqr ? math.normalize(forward) : new float3(0, 0, 1);
                    }

                    foreach (AOIEntity aoiEntity in caster.GetBeSeePlayers().Values)
                    {
                        Unit targetUnit = aoiEntity.GetParent<Unit>();
                        if (targetUnit == caster)
                        {
                            continue;
                        }

                        float3 toTarget = targetUnit.Position - caster.Position;
                        toTarget.y = 0;
                        if (math.lengthsq(toTarget) <= minDirectionSqr)
                        {
                            continue;
                        }

                        float3 toTargetDir = math.normalize(toTarget);
                        float dot = math.clamp(math.dot(forward, toTargetDir), -1f, 1f);
                        float angleToTarget = math.degrees(math.acos(dot));
                        if (angleToTarget <= halfAngle && IsMatchSelectCampType(caster, targetUnit, selectCampType))
                        {
                            cast.Targets.Add(targetUnit.Id);
                            nowCount++;
                        }

                        if (nowCount >= needCount)
                        {
                            break;
                        }
                    }

                    break;
                }
            }

        }

        private static SelectCampType GetSingleSelectCampType(Cast cast)
        {
            if (cast.Config.SelectParam != null && cast.Config.SelectParam.Length > 1)
            {
                return (SelectCampType)cast.Config.SelectParam[1];
            }

            return (SelectType)cast.Config.SelectType switch
            {
                SelectType.FriendlyTarget => SelectCampType.Ally,
                SelectType.EnemyTarget => SelectCampType.Hostile,
                _ => SelectCampType.Ally,
            };
        }

        private static bool IsMatchSelectCampType(Unit caster, Unit target, SelectCampType selectCampType)
        {
            return selectCampType switch
            {
                SelectCampType.Ally => CampHelper.IsAlly(caster, target),
                SelectCampType.Hostile => CampHelper.IsHostile(caster, target),
                _ => false,
            };
        }

    }
}
