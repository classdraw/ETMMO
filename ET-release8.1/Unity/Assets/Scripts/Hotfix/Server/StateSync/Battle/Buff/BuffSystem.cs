using System;

namespace ET.Server
{
    [EntitySystemOf(typeof(Buff))]
    [FriendOf(typeof(Buff))]
    public static partial class BuffSystem
    {
        [EntitySystem]
        private static void Awake(this ET.Server.Buff self, int configId)
        {
            self.ConfigId = configId;
            self.AddComponent<ActionsTempComponent>();
        }

        [EntitySystem]
        private static void Destroy(this ET.Server.Buff self)
        {
            self.ConfigId = 0;
            self.Owner = null;
            self.AddUnitId = 0;
            self.AddSkillId = 0;
            self.CreateTime = 0;
            self.TickTime = 0;
            self.TickBeginTime = 0;
            self.ExpireTime = 0;
            self.Layer = 0;

            self.Root().GetComponent<TimerComponent>().Remove(ref self.TickTimer);
            self.TickTimer = 0;
            
            self.Root().GetComponent<TimerComponent>().Remove(ref self.WaitTickTimer);
            self.WaitTickTimer = 0;
            
            self.Root().GetComponent<TimerComponent>().Remove(ref self.ExpireTimer);
            self.ExpireTimer = 0;
        }
        
        [EntitySystem]
        private static void Deserialize(this ET.Server.Buff self)
        {
            self.AddComponent<ActionsTempComponent>();
            self.Owner = self.Parent?.GetComponent<Unit>();
        }

        public static void InitTime(this Buff self, int totalTime)
        {
            long now = TimeInfo.Instance.ServerFrameTime();
            self.CreateTime = now;
            self.TickBeginTime = now;
            self.ExpireTime = totalTime > 0 ? now + totalTime : 0;
        }
        public static void InitLayer(this Buff self, int firstAddLayer, int layerLimit)
        {
            self.Layer = ClampLayer(firstAddLayer, layerLimit);
        }
        /// <summary>
        /// 时长叠加：在剩余时间基础上继续增加，可越叠越长。
        /// </summary>
        public static void AddTotalTime(this Buff self, int totalTime)
        {
            if (totalTime <= 0)
            {
                return;
            }

            long now = TimeInfo.Instance.ServerFrameTime();
            long baseTime = self.ExpireTime > now ? self.ExpireTime : now;
            self.ExpireTime = baseTime + totalTime;
        }

        /// <summary>
        /// 时长重置：从当前时刻重新计算配置时长，不叠加。
        /// </summary>
        public static void ResetTotalTime(this Buff self, int durationMs)
        {
            long now = TimeInfo.Instance.ServerFrameTime();
            self.TickBeginTime = now;
            self.ExpireTime = durationMs > 0 ? now + durationMs : 0;
        }

        public static void AddLayer(this Buff self, int addLayer, int layerLimit)
        {
            if (addLayer <= 0)
            {
                return;
            }

            self.Layer += addLayer;
            if (layerLimit > 0 && self.Layer > layerLimit)
            {
                self.Layer = layerLimit;
            }
        }

        public static int ClampLayer(int layer, int layerLimit)
        {
            if (layerLimit > 0 && layer > layerLimit)
            {
                return layerLimit;
            }

            return layer;
        }



        public static BuffProto ToMessage(this Buff self)
        {
            BuffProto buffProto = BuffProto.Create(true);
            buffProto.Id = self.Id;
            buffProto.ConfigId = self.ConfigId;
            buffProto.ExpireTime = self.ExpireTime;
            buffProto.CreateTime = self.CreateTime;
            buffProto.ExtraData = self.ToExtraDataBytes();
            return buffProto;
        }

        public static void FromMessage(this Buff self, BuffProto buffProto)
        {
            if (buffProto == null)
            {
                return;
            }

            self.ConfigId = buffProto.ConfigId;
            self.CreateTime = buffProto.CreateTime;
            self.ExpireTime = buffProto.ExpireTime;
            self.FromExtraDataBytes(buffProto.ExtraData);
        }

        private static byte[] ToExtraDataBytes(this Buff self)
        {
            BuffExtraData extraData = new BuffExtraData
            {
                AddUnitId = self.AddUnitId,
                AddSkillId = self.AddSkillId,
                TickTime = self.TickTime,
                TickBeginTime = self.TickBeginTime,
                Layer = self.Layer,
            };
            return MongoHelper.Serialize(extraData);
        }

        private static void FromExtraDataBytes(this Buff self, byte[] bytes)
        {
            if (bytes == null || bytes.Length == 0)
            {
                self.AddUnitId = 0;
                self.AddSkillId = 0;
                self.TickTime = 0;
                self.TickBeginTime = 0;
                self.Layer = 0;
                return;
            }

            BuffExtraData extraData = MongoHelper.Deserialize<BuffExtraData>(bytes);
            self.AddUnitId = extraData.AddUnitId;
            self.AddSkillId = extraData.AddSkillId;
            self.TickTime = extraData.TickTime;
            self.TickBeginTime = extraData.TickBeginTime;
            self.Layer = extraData.Layer;
        }
        
        
        #region  buffActions逻辑
        //新增Buff 逻辑迭代
        public static void AddActions(this Buff self)
        {
            long instanceId = self.InstanceId;
            foreach (var actionId in self.Config.AddAction)
            {
                try
                {
                    self.CreateActions(actionId, ActionsRunType.BuffAdd);
                    //可能在执行效果的过程中，本buff移除回池了，然后从池子里又取出来，所有只判断isDisposed不够
                    if (self.InstanceId!=instanceId)
                    {
                        break;
                    }
                    
                }
                catch (Exception e)
                {
                    Log.Error($"AddActions Error {self.ConfigId} {actionId} {e} !!!");
                }
            }
        }
        //移除Buff 逻辑迭代 
        public static void RemoveActions(this Buff self)
        {
            long instanceId = self.InstanceId;
            foreach (var actionId in self.Config.RemoveAction)
            {
                try
                {
                    self.CreateActions(actionId, ActionsRunType.BuffRemove);
                    //可能在执行效果的过程中，本buff移除回池了，然后从池子里又取出来，所有只判断isDisposed不够
                    if (self.InstanceId!=instanceId)
                    {
                        break;
                    }
                    
                }
                catch (Exception e)
                {
                    Log.Error($"RemoveActions Error {self.ConfigId} {actionId} {e} !!!");
                }
            }
        }
        //Buff Action的逻辑迭代
        public static void TickActions(this Buff self)
        {
            if (self.IsDisposed)
            {
                return;
            }
            
            long instanceId = self.InstanceId;
            foreach (var actionId in self.Config.TickAction)
            {
                try
                {
                    self.CreateActions(actionId, ActionsRunType.BuffTick);
                    //可能在执行效果的过程中，本buff移除回池了，然后从池子里又取出来，所有只判断isDisposed不够
                    if (self.InstanceId!=instanceId)
                    {
                        break;
                    }
                    
                }
                catch (Exception e)
                {
                    if (instanceId==self.InstanceId)
                    {
                        Log.Error($"TickActions ErrorAAA {self.ConfigId} {actionId} {e} !!!"); 
                    }
                    else
                    {
                        Log.Error($"TickActions ErrorBBB {actionId} {e} !!!"); 
                    }
                }
            }
            
            if (instanceId!=self.InstanceId)
            {
                return;
            }

            if (self.Config.TickAction.Length>0)
            {
                Unit owner = self.Owner;
                if (owner!=null&&!owner.IsDisposed)
                {
                    M2C_BuffTick m2CBuffTick = M2C_BuffTick.Create();
                    m2CBuffTick.BuffId = self.Id;
                    m2CBuffTick.UnitId = owner.Id;
                    MapMessageHelper.SendClient(owner, m2CBuffTick, (NoticeClientType)self.Config.NoticeClientType);
                }


            }

        }

        #endregion
    }
}
