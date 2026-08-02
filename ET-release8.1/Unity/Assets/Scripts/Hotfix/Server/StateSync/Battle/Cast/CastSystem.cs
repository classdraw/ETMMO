using System.Collections.Generic;
using Unity.Mathematics;

namespace ET.Server
{
    [EntitySystemOf(typeof(Cast))]
    [FriendOf(typeof(Cast))]
    [FriendOf(typeof(SkillStatusComponent))]
    public static partial class CastSystem
    {
        private const float MinTurnDirectionSqr = 0.01f;

        [EntitySystem]
        private static void Awake(this ET.Server.Cast self,int configId)
        {
            self.ConfigId = configId;
            self.AddComponent<ActionsTempComponent>();
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
            int err = cast.RefreshTargets();
            if (err != ErrorCode.ERR_Success)
            {
                cast.Dispose();
                return err;
            }

            cast.StartTime = TimeInfo.Instance.ServerFrameTime();
            Unit caster = cast.Caster;
            caster?.GetComponent<SkillStatusComponent>()?.BeginCurrentSkill(cast);
            cast.CastBeginAsync().Coroutine();
            return ErrorCode.ERR_Success;
        }

        /// <summary>
        /// 校验输入、重新选目标、校验目标数量
        /// </summary>
        public static int RefreshTargets(this Cast cast)
        {
            int err = cast.CastCheck();
            if (err != ErrorCode.ERR_Success)
            {
                return err;
            }

            cast.SelectTargets();
            return cast.CastCheckBeforeBegin();
        }

        private static int CastCheck(this Cast cast)
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
            if (selectType==SelectType.Self)
            {
                return ErrorCode.ERR_Success;
            }

            int[] selectParam = cast.Config.SelectParam;

            if (selectType==SelectType.Position)
            {
                int[] shapeParam = cast.Config.ShapeParam;
                if (shapeParam == null || shapeParam.Length < 1 || (ShapeType)shapeParam[0] == ShapeType.Single)
                {
                    return ErrorCode.ERR_CastConfigError;
                }

                int err = CheckCastRange(caster, cast.InputPos, selectParam);
                if (err != ErrorCode.ERR_Success)
                {
                    return err;
                }

                return ErrorCode.ERR_Success;
            }

            Unit inputUnit = caster.Scene().GetComponent<UnitComponent>().Get(cast.InputUnitId);
            if (inputUnit == null || inputUnit.IsDisposed||!inputUnit.IsBattleUnit())
            {
                return ErrorCode.ERR_CastInputUnitError;
            }

            int rangeErr = CheckCastRange(caster, inputUnit.Position, selectParam);
            if (rangeErr != ErrorCode.ERR_Success)
            {
                return rangeErr;
            }
            
            switch (selectType)
            {
                case SelectType.FriendlyTarget:
                {
                    if (CampHelper.IsHostile(caster,inputUnit))
                    {
                        return ErrorCode.ERR_CastInputUnitError;
                    }
                    break;
                }
                case SelectType.EnemyTarget:
                {
                    if (CampHelper.IsAlly(caster,inputUnit))
                    {
                        return ErrorCode.ERR_CastInputUnitError;
                    }
                    break;
                }
            }

            cast.InputUnit = inputUnit;
            return ErrorCode.ERR_Success;
        }

        /// <summary>
        /// 校验施法距离。SelectParam 为空或 SelectParam[0]==0 表示不限制距离。
        /// </summary>
        private static int CheckCastRange(Unit caster, float3 targetPos, int[] selectParam)
        {
            if (selectParam == null || selectParam.Length < 1 || selectParam[0] == 0)
            {
                return ErrorCode.ERR_Success;
            }

            if (selectParam[0] < 0)
            {
                return ErrorCode.ERR_CastConfigError;
            }

            float maxRange = selectParam[0] / 1000f;
            float3 offset = targetPos - caster.Position;
            offset.y = 0;
            if (math.lengthsq(offset) > maxRange * maxRange)
            {
                return ErrorCode.ERR_CastOutOfRangeError;
            }

            return ErrorCode.ERR_Success;
        }

        private static int CastCheckBeforeBegin(this Cast cast)
        {
            SelectType selectType = (SelectType)cast.Config.SelectType;
            switch (selectType)
            {
                case SelectType.Self:
                case SelectType.FriendlyTarget:
                case SelectType.EnemyTarget:
                    if (cast.Targets.Count<=0)
                    {
                        return ErrorCode.ERR_CastNoTargetError;
                        
                    }
                    break;
                case SelectType.Position:
                    break;
            }
            return ErrorCode.ERR_Success;
        }
        


        
        private static void SelectTargets(this Cast cast)
        {
            Unit caster = cast.Caster;
            cast.Targets.Clear();
            SelectType selectType = (SelectType)cast.Config.SelectType;
            switch (selectType)
            {
                //不处理none，属于异常
                case SelectType.Self:
                    cast.SelectTargetsInner(caster, caster.Position);
                    break;
                case SelectType.FriendlyTarget:
                case SelectType.EnemyTarget:
                    cast.SelectTargetsInner(cast.InputUnit, caster.Position);
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
            ShapeType shapeType =(ShapeType)cast.Config.ShapeParam[0];
            if (shapeType==ShapeType.Single)
            {
                if (ShapeSelectHelper.TrySelectSingle(unit, pos, cast.Config.ShapeParam[1]))
                {
                    cast.Targets.Add(unit.Id);
                }

                return;
            }

            if (unit!=null)
            {
                pos = unit.Position;//根据这个坐标进行筛选
            }

            using ListComponent<Unit> list = ListComponent<Unit>.Create();
            switch (shapeType)
            {
                case ShapeType.Circle://圆形
                {
                    ShapeSelectHelper.SelectCircle(caster, pos, cast.Config.ShapeParam[1], cast.Config.ShapeParam[2],
                        (SelectCampType)cast.Config.ShapeParam[3], caster.GetAoiUnits(), list);
                    break;
                }
                case ShapeType.Rectangle://矩形
                {
                    ShapeSelectHelper.SelectRectangle(caster, pos, cast.Config.ShapeParam[1], cast.Config.ShapeParam[2],
                        cast.Config.ShapeParam[3], cast.Config.ShapeParam[4], (SelectCampType)cast.Config.ShapeParam[5],
                        caster.GetAoiUnits(), list);
                    break;
                }
                case ShapeType.Fan://扇形
                {
                    ShapeSelectHelper.SelectFan(caster, pos, cast.Config.ShapeParam[1], cast.Config.ShapeParam[2],
                        cast.Config.ShapeParam[3], (SelectCampType)cast.Config.ShapeParam[4], caster.GetAoiUnits(), list);
                    break;
                }
            }

            foreach (Unit targetUnit in list)
            {
                cast.Targets.Add(targetUnit.Id);
            }
        }

        
        /// <summary>
        /// 技能释放
        /// </summary>
        /// <param name="cast"></param>
        public static async ETTask CastBeginAsync(this Cast cast)
        {
            Unit caster = cast.Caster;
            cast.TurnToCastPosition();
            M2C_CastStart m2CCastStart = M2C_CastStart.Create();
            m2CCastStart.CasterId = caster.Id;
            m2CCastStart.CastId = cast.Id;
            m2CCastStart.CastConfigId = cast.ConfigId;
            m2CCastStart.Forward = caster.Forward;
            m2CCastStart.TargetsId = new List<long>();
            m2CCastStart.TargetsId.AddRange(cast.Targets);
            
            MapMessageHelper.SendClient(caster,m2CCastStart,(NoticeClientType)cast.Config.NoticeClientType);

            CastConfig config = cast.Config;
            if (config.Times.Count <= 0)
            {
                if (config.TotalTime > 0)//如果没有配置times 理论上totaltime应该是0 如果配置，那么就是需要延迟销毁
                {
                    long castInstaceId = cast.InstanceId;
                    long casterInstanceId = caster.InstanceId;
                    await cast.Root().GetComponent<TimerComponent>().WaitTillAsync(cast.StartTime + config.TotalTime);
                    if (!cast.CheckAsyncInvalid(castInstaceId, casterInstanceId))
                    {
                        return;
                    }
                }

                cast?.CastFinish();
                return;
            }
            
            foreach (int time in config.Times)
            {
                long castInstaceId = cast.InstanceId;
                long casterInstanceId = caster.InstanceId;
                //技能事件时间点
                await cast.Root().GetComponent<TimerComponent>().WaitTillAsync(cast.StartTime + time);
                
                if (!cast.CheckAsyncInvalid(castInstaceId,casterInstanceId))
                {
                    Log.Warning($"Cast AsyncInvalid {castInstaceId} {casterInstanceId} Action");
                    return;
                }
                //创建技能行为实体
                foreach (CastActionTimes castActionTimes in config.TimesDict[time])
                {
                    if (castActionTimes.IsSelfHit)
                    {
                        cast.HandleSelfHit(castActionTimes.Index);
                    }
                    else
                    {
                        cast.HandleTargetHit(castActionTimes.Index);
                    }
                }
            }

            if (config.TotalTime>0)
            {
                long castInstaceId = cast.InstanceId;
                long casterInstanceId = caster.InstanceId;
                await cast.Root().GetComponent<TimerComponent>().WaitTillAsync(cast.StartTime + config.TotalTime);
                if (!cast.CheckAsyncInvalid(castInstaceId,casterInstanceId))
                {
                    Log.Warning($"Cast AsyncInvalid {castInstaceId} {casterInstanceId} Over");
                    return;
                }
                
            }
            cast?.CastFinish();

            await ETTask.CompletedTask;
        }


        public static void HandleSelfHit(this Cast cast, int index)
        {
            int[] actions = cast.Config.SelfHitAction;
            if (index >= 0 && index <= actions.Length-1)
            {
                int actionId = actions[index];
                if (actionId!=0)
                {
                    cast.HandleHit(actionId,true,index);
                }
            }

            int[] selfHitBuffs = cast.Config.SelfHitBuffs;
            if (index>=0&& index<=selfHitBuffs.Length-1)
            {
                int buffId = selfHitBuffs[index];
                if (buffId!=0)
                {
                    Unit caster = cast.Caster;
                    caster.GetComponent<BuffComponent>()?.CreateAndAdd(buffId,caster.Id,cast.ConfigId);
                }
            }
        }

        public static void HandleTargetHit(this Cast cast, int index)
        {
            int[] actions = cast.Config.HitAction;
            if (index >=0 && index <= actions.Length-1)
            {
                int actionId = actions[index];
                if (actionId!=0)
                {
                    cast.HandleHit(actionId, false,index);
                }
            }
            
            int[] hitBuffs = cast.Config.HitBuffs;
            if (index>=0&&index<=hitBuffs.Length-1)
            {
                int buffId = hitBuffs[index];
                if (buffId!=0)
                { 
                    Unit caster = cast.Caster;
                    UnitComponent unitComponent = caster.Scene().GetComponent<UnitComponent>();
                    List<long> targets = new List<long>(cast.Targets);
                    foreach (long targetId in targets)
                    {
                        Unit target = unitComponent.Get(targetId);
                        if (target == null || target.IsDisposed || !target.IsBattleUnit())
                        {
                            continue;
                        }
                    
                        target.GetComponent<BuffComponent>()?.CreateAndAdd(buffId,caster.Id,cast.ConfigId);
                    }

                }
            }

        }

        private static void HandleHit(this Cast cast, int actionId, bool selfHit,int hitIndex)
        {
            if (cast.RefreshTargets() != ErrorCode.ERR_Success)
            {
                return;
            }

            Unit caster = cast.Caster;
            M2C_CastHit m2CCastHit = M2C_CastHit.Create();
            m2CCastHit.CasterId = caster.Id;
            m2CCastHit.CastId = cast.Id;
            m2CCastHit.HitIndex = hitIndex;
            m2CCastHit.IsSelf = selfHit;
            m2CCastHit.TargetsId = new List<long>();
            m2CCastHit.TargetsId.AddRange(cast.Targets);
            MapMessageHelper.SendClient(caster,m2CCastHit,(NoticeClientType)cast.Config.NoticeClientType);
            
            if (selfHit)
            {
                cast.CreateActions(actionId, caster, ActionsRunType.CastHit);
            }
            else
            {
                UnitComponent unitComponent = caster.Scene().GetComponent<UnitComponent>();
                foreach (long targetId in cast.Targets)
                {
                    Unit target = unitComponent.Get(targetId);
                    if (target == null || target.IsDisposed || !target.IsBattleUnit())
                    {
                        continue;
                    }
                    
                    cast.CreateActions(actionId, target, ActionsRunType.CastHit);
                }
            }


        }

        public static void CastFinish(this Cast cast)
        {
            if (cast == null || cast.IsDisposed)
            {
                return;
            }

            Unit caster = cast.Caster;
            if (caster != null && !caster.IsDisposed)
            {
                caster.GetComponent<SkillStatusComponent>()?.ClearCurrentSkill(cast);

                if (cast.Config.TotalTime > 0)
                {
                    M2C_CastFinish castFinish = M2C_CastFinish.Create();
                    castFinish.CasterId = caster.Id;
                    castFinish.CastId = cast.Id;
                    MapMessageHelper.SendClient(caster, castFinish, (NoticeClientType)cast.Config.NoticeClientType);
                }
            }

            cast.Dispose();
        }

        public static void CastBreak(this Cast cast)
        {
            if (cast == null || cast.IsDisposed)
            {
                return;
            }

            Unit caster = cast.Caster;
            if (caster != null && !caster.IsDisposed)
            {
                caster.GetComponent<SkillStatusComponent>()?.ClearCurrentSkill(cast);

                Log.Console($"[Cast] 玩家 {caster.Id} 技能 {cast.Id}({cast.ConfigId}) 发送 M2C_CastBreak");
                M2C_CastBreak castBreak = M2C_CastBreak.Create();
                castBreak.CasterId = caster.Id;
                castBreak.CastId = cast.Id;
                MapMessageHelper.SendClient(caster, castBreak, (NoticeClientType)cast.Config.NoticeClientType);
            }

            cast.Dispose();
        }

        /// <summary>
        /// 施法前转向施法位置（目标单位或有效输入坐标）。
        /// </summary>
        public static void TurnToCastPosition(this Cast cast)
        {
            if (!cast.Config.NeedLookTarget)
            {
                return;
            }

            Unit caster = cast.Caster;
            if (caster == null || caster.IsDisposed)
            {
                return;
            }

            if (!cast.TryGetCastTurnPosition(out float3 castPos))
            {
                return;
            }

            float3 direction = castPos - caster.Position;
            direction.y = 0;
            if (math.lengthsq(direction) <= MinTurnDirectionSqr)
            {
                return;
            }

            caster.Forward = math.normalize(direction);
        }

        private static bool TryGetCastTurnPosition(this Cast cast, out float3 castPos)
        {
            castPos = default;

            Unit inputUnit = cast.InputUnit;
            if (inputUnit != null && !inputUnit.IsDisposed)
            {
                castPos = inputUnit.Position;
                return true;
            }

            SelectType selectType = (SelectType)cast.Config.SelectType;
            if (selectType == SelectType.Position)
            {
                castPos = cast.InputPos;
                return true;
            }

            Unit caster = cast.Caster;
            if (caster == null || caster.IsDisposed)
            {
                return false;
            }

            float3 direction = cast.InputPos - caster.Position;
            direction.y = 0;
            if (math.lengthsq(direction) > MinTurnDirectionSqr)
            {
                castPos = cast.InputPos;
                return true;
            }

            return false;
        }

        //检测技能异步结束后是否合法
        public static bool CheckAsyncInvalid(this Cast cast,long castInstanceId,long casterInstanceId)
        {
            if (cast==null||cast.IsDisposed)
            {
                return false;
            }
            Unit caster = cast.Caster;
            if (caster==null||caster.IsDisposed)
            {
                return false;
            }

            if (cast.InstanceId!=castInstanceId||caster.InstanceId!=casterInstanceId)
            {
                return false;
            }

            return true;
        }
    }
}
