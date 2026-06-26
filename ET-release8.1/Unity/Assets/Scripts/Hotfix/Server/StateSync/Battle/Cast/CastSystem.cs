using System.Collections.Generic;
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
                return err;
            }

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
            if (selectType==SelectType.Self)//选自己肯定没问题
            {
                return ErrorCode.ERR_Success;
            }
            
            if (selectType==SelectType.Position)
            {
                ShapeType shapeType =(ShapeType)cast.Config.SelectParam[0];
                if (shapeType==ShapeType.Single)//选的是坐标 必须有选择器
                {
                    return ErrorCode.ERR_Success;
                }
            }
            
            Unit inputUnit = caster.Scene().GetComponent<UnitComponent>().Get(cast.InputUnitId);
            if (inputUnit == null || inputUnit.IsDisposed||!inputUnit.IsBattleUnit())
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
                if (ShapeSelectHelper.TrySelectSingle(unit, pos, cast.Config.SelectParam[1]))
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
                    ShapeSelectHelper.SelectCircle(caster, pos, cast.Config.SelectParam[1], cast.Config.SelectParam[2],
                        (SelectCampType)cast.Config.SelectParam[3], caster.GetAoiUnits(), list);
                    break;
                }
                case ShapeType.Rectangle://矩形
                {
                    ShapeSelectHelper.SelectRectangle(caster, pos, cast.Config.SelectParam[1], cast.Config.SelectParam[2],
                        cast.Config.SelectParam[3], cast.Config.SelectParam[4], (SelectCampType)cast.Config.SelectParam[5],
                        caster.GetAoiUnits(), list);
                    break;
                }
                case ShapeType.Fan://扇形
                {
                    ShapeSelectHelper.SelectFan(caster, pos, cast.Config.SelectParam[1], cast.Config.SelectParam[2],
                        cast.Config.SelectParam[3], (SelectCampType)cast.Config.SelectParam[4], caster.GetAoiUnits(), list);
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
            //技能开始消息
            cast.StartTime = TimeInfo.Instance.ServerFrameTime();
            M2C_CastStart m2CCastStart = M2C_CastStart.Create();
            m2CCastStart.CasterId = caster.Id;
            m2CCastStart.CastId = cast.Id;
            m2CCastStart.CastConfigId = cast.ConfigId;
            m2CCastStart.TargetsId = new List<long>();
            m2CCastStart.TargetsId.AddRange(cast.Targets);
            
            MapMessageHelper.SendClient(caster,m2CCastStart,(NoticeClientType)cast.Config.NoticeClientType);

            CastConfig config = cast.Config;
            if (config.Times.Count<=0)
            {
                return;
            }

            long castInstaceId = 0;
            long casterInstanceId = 0;
            foreach (int time in config.Times)
            {
                castInstaceId = cast.InstanceId;
                casterInstanceId = caster.InstanceId;
                //技能事件时间点
                await cast.Root().GetComponent<TimerComponent>().WaitTillAsync(cast.StartTime + time);
                
                if (!cast.CheckAsyncInvalid(castInstaceId,casterInstanceId))
                {
                    Log.Error($"Cast AsyncInvalid {castInstaceId} {casterInstanceId} Action");
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
                castInstaceId = cast.InstanceId;
                casterInstanceId = caster.InstanceId;
                await cast.Root().GetComponent<TimerComponent>().WaitTillAsync(cast.StartTime + config.TotalTime);
                if (!cast.CheckAsyncInvalid(castInstaceId,casterInstanceId))
                {
                    Log.Error($"Cast AsyncInvalid {castInstaceId} {casterInstanceId} Over");
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
                    cast.HandleHit(actionId, selfHit: true);
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
                    cast.HandleHit(actionId, selfHit: false);
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
                    foreach (long targetId in cast.Targets)
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

        private static void HandleHit(this Cast cast, int actionId, bool selfHit)
        {
            if (cast.RefreshTargets() != ErrorCode.ERR_Success)
            {
                return;
            }

            Unit caster = cast.Caster;
            M2C_CastHit m2CCastHit = M2C_CastHit.Create();
            m2CCastHit.CasterId = caster.Id;
            m2CCastHit.CastId = cast.Id;
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
            //没有持续事件，瞬发技能，不用通知
            if (cast.Config.TotalTime>0)
            {
                Unit caster = cast.Caster;
                M2C_CastFinish castFinish = M2C_CastFinish.Create();
                castFinish.CasterId = caster.Id;
                castFinish.CastId = cast.Id;
                MapMessageHelper.SendClient(caster,castFinish,(NoticeClientType)cast.Config.NoticeClientType);
            }
            cast?.Dispose();
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
