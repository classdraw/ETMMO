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
            self.inputUnitId = 0;
            self.inputPos = default;
            self.StartTime = 0;
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
            
            SelectType selectType = (SelectType)cast.Config.selectType;
            if (selectType==SelectType.None)
            {
                return ErrorCode.ERR_CastConfigError;
            }

            if (selectType==SelectType.Self||selectType==SelectType.Position)
            {
                return ErrorCode.ERR_Success;
            }


            Unit inputUnit = caster.Scene().GetComponent<UnitComponent>().Get(cast.inputUnitId);
            if (inputUnit == null || inputUnit.IsDisposed)
            {
                return ErrorCode.ERR_CastInputUnitError;
            }
            
            switch (selectType)
            {
                //需要一个目标，那么前端需要给一个目标
                case SelectType.FriendlyTarget:

                case SelectType.EnemyTarget:
                    
                    break;
            }

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
            cast.Targets.Clear();
            SelectType selectType = (SelectType)cast.Config.selectType;
            switch (selectType)
            {
                //不处理none，属于异常
                case SelectType.Self:
                    SelectTargetsSelf(cast);
                    break;
                case SelectType.FriendlyTarget:
                    SelectTargetsSingle(cast);
                    break;
                case SelectType.EnemyTarget:
                    SelectTargetsSelfFan(cast);
                    break;
                case SelectType.Position:
                    SelectTargetsPosition(cast);
                    break;
                default:
                    Log.Error($"未知目标选择类型: {selectType}");
                    break;
            }
        }

        private static void SelectTargetsSelf(Cast cast)
        {
        }

        private static void SelectTargetsSingle(Cast cast)
        {
        }

        private static void SelectTargetsSelfFan(Cast cast)
        {
        }

        private static void SelectTargetsPosition(Cast cast)
        {
        }
    }
}
