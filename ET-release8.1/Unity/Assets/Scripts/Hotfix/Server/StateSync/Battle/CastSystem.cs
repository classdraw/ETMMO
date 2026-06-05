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
                    CastHelper.SelectTargetsSelf(cast);
                    break;
                case SelectType.Single:
                    CastHelper.SelectTargetsSingle(cast);
                    break;
                case SelectType.SelfFan:
                    CastHelper.SelectTargetsSelfFan(cast);
                    break;
                case SelectType.SelfRectangle:
                    CastHelper.SelectTargetsSelfRectangle(cast);
                    break;
                case SelectType.SelfFanRectangle:
                    CastHelper.SelectTargetsSelfFanRectangle(cast);
                    break;
                case SelectType.DstFan:
                    CastHelper.SelectTargetsDstFan(cast);
                    break;
                case SelectType.DstRectangle:
                    CastHelper.SelectTargetsDstRectangle(cast);
                    break;
                case SelectType.DstFanRectangle:
                    CastHelper.SelectTargetsDstFanRectangle(cast);
                    break;
                case SelectType.Position:
                    CastHelper.SelectTargetsPosition(cast);
                    break;
                default:
                    Log.Error($"未知目标选择类型: {selectType}");
                    break;
            }
        }
    }
}
