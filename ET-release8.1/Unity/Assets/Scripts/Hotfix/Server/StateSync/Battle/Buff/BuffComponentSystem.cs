using System;

namespace ET.Server
{
    [EntitySystemOf(typeof(BuffComponent))]
    [FriendOf(typeof(BuffComponent))]
    [FriendOf(typeof(Buff))]
    [FriendOf(typeof(BuffCreateInfo))]
    public static partial class BuffComponentSystem
    {
        private enum BuffCoverHandleResult
        {
            NeedCreate,
            Handled,
            Rejected,
        }

        [EntitySystem]
        private static void Awake(this ET.Server.BuffComponent self)
        {
            self.AddComponent<BuffTempComponent>();
        }

        [EntitySystem]
        private static void Destroy(this ET.Server.BuffComponent self)
        {
            self.BuffsDict.Clear();
        }

        [EntitySystem]
        private static void Deserialize(this ET.Server.BuffComponent self)
        {
            self.BuffsDict.Clear();
            self.AddComponent<BuffTempComponent>();
            foreach (Entity entity in self.Children.Values)
            {
                if (entity is Buff buff)
                {
                    self.BuffsDict.Add(buff.Id,buff);
                }
            }
        }

        private static BuffCreateInfo CreateBuffInfo(this ET.Server.BuffComponent self, int configId, long addUnitId, int addSkillId, int firstLayer = 0)
        {
            BuffCreateInfo buffCreateInfo = self.GetComponent<BuffTempComponent>().AddChild<BuffCreateInfo, int>(configId);
            buffCreateInfo.AddUnitId = addUnitId;
            buffCreateInfo.AddSkillId = addSkillId;
            buffCreateInfo.FirstLayer = firstLayer;
            return buffCreateInfo;
        }

        public static bool CreateAndAdd(this ET.Server.BuffComponent self, int configId, long addUnitId, int addSkillId, int firstLayer = 0)
        {
            using (BuffCreateInfo buffCreateInfo = self.CreateBuffInfo(configId, addUnitId, addSkillId, firstLayer))
            {
                return self.Add(buffCreateInfo);
            }
        }

        public static bool Add(this ET.Server.BuffComponent self, BuffCreateInfo buffCreateInfo)
        {
            if (buffCreateInfo == null || buffCreateInfo.IsDisposed)
            {
                return false;
            }

            if (self == null || self.IsDisposed)
            {
                return false;
            }

            Unit owner = self.GetParent<Unit>();
            if (owner == null || owner.IsDisposed)
            {
                return false;
            }

            BuffCoverHandleResult result = self.HandleBuffCover(buffCreateInfo, out Buff handledBuff);
            switch (result)
            {
                case BuffCoverHandleResult.Handled:
                    if (handledBuff != null)//保持旧的buff，但是layer，time有变化需要通知客户端 
                    {
                        self.NotifyBuffUpdate(handledBuff);
                    }
                    return true;
                case BuffCoverHandleResult.Rejected:
                    //某些逻辑阻挡这个buff创建
                    return false;
                case BuffCoverHandleResult.NeedCreate:
                    self.CreateBuff(buffCreateInfo, owner);
                    return true;
                default:
                    return false;
            }
        }

        private static BuffCoverHandleResult HandleBuffCover(this BuffComponent self, BuffCreateInfo buffCreateInfo, out Buff handledBuff)
        {
            handledBuff = null;
            BuffConfig buffConfig = BuffConfigCategory.Instance.Get(buffCreateInfo.ConfigId);
            int configId = buffCreateInfo.ConfigId;
            int totalTime = buffConfig.TotalTime;
            BuffCoverType coverType = (BuffCoverType)buffConfig.ConverType;
            Buff oldBuff = self.FindCoverTarget(configId, coverType, buffCreateInfo);

            switch (coverType)
            {
                case BuffCoverType.AddTime:
                {
                    if (oldBuff == null)
                    {
                        return BuffCoverHandleResult.NeedCreate;
                    }

                    oldBuff.AddTotalTime(totalTime);
                    oldBuff.AddLayer(self.GetFirstAddLayer(buffCreateInfo, buffConfig), buffConfig.LayerLimit);
                    handledBuff = oldBuff;
                    return BuffCoverHandleResult.Handled;
                }
                case BuffCoverType.Replace:
                {
                    if (oldBuff == null)
                    {
                        return BuffCoverHandleResult.NeedCreate;
                    }

                    int newLayer = self.GetInitialLayer(buffCreateInfo, buffConfig);
                    if (newLayer <= oldBuff.Layer)
                    {
                        return BuffCoverHandleResult.Handled;
                    }

                    self.UnregisterBuff(oldBuff);
                    return BuffCoverHandleResult.NeedCreate;
                }
                case BuffCoverType.ResetTime:
                {
                    if (oldBuff == null)
                    {
                        return BuffCoverHandleResult.NeedCreate;
                    }

                    oldBuff.ResetTotalTime(totalTime);
                    oldBuff.AddLayer(self.GetFirstAddLayer(buffCreateInfo, buffConfig), buffConfig.LayerLimit);
                    handledBuff = oldBuff;
                    return BuffCoverHandleResult.Handled;
                }
                case BuffCoverType.New:
                    return BuffCoverHandleResult.NeedCreate;
                case BuffCoverType.SelfMutex:
                    return oldBuff == null ? BuffCoverHandleResult.NeedCreate : BuffCoverHandleResult.Rejected;
                case BuffCoverType.Role:
                {
                    if (oldBuff == null)
                    {
                        return BuffCoverHandleResult.NeedCreate;
                    }

                    oldBuff.ResetTotalTime(totalTime);
                    oldBuff.AddLayer(self.GetFirstAddLayer(buffCreateInfo, buffConfig), buffConfig.LayerLimit);
                    handledBuff = oldBuff;
                    return BuffCoverHandleResult.Handled;
                }
                case BuffCoverType.ClassifyMutex:
                    return self.HasClassifyTypeBuff(buffConfig.Type)
                        ? BuffCoverHandleResult.Rejected
                        : BuffCoverHandleResult.NeedCreate;
                default:
                    Log.Error($"未知BuffCoverType: {coverType}, configId: {configId}");
                    return BuffCoverHandleResult.Rejected;
            }
        }

        /// <summary>
        /// 按 Buff 实例唯一 Id 移除（New 等同 ConfigId 多实例时使用）
        /// </summary>
        public static void Remove(this BuffComponent self, long buffId)
        {
            Buff buff = self.Get(buffId);
            if (buff == null)
            {
                return;
            }

            self.UnregisterBuff(buff);
        }
        
        /// <summary>
        /// 按 Buff 实例唯一 Id 获取
        /// </summary>
        public static Buff Get(this BuffComponent self, long buffId)
        {
            if (!self.BuffsDict.TryGetValue(buffId, out EntityRef<Buff> buffRef))
            {
                return null;
            }

            return buffRef;
        }
        

        public static Buff GetByRole(this BuffComponent self, int configId, long addUnitId)
        {
            foreach (Entity entity in self.Children.Values)
            {
                if (entity is not Buff buff)
                {
                    continue;
                }

                if (buff.ConfigId == configId && buff.AddUnitId == addUnitId)
                {
                    return buff;
                }
            }

            return null;
        }

        private static bool HasClassifyTypeBuff(this BuffComponent self, int classifyType)
        {
            foreach (Entity entity in self.Children.Values)
            {
                if (entity is Buff buff && buff.Config.Type == classifyType)
                {
                    return true;
                }
            }

            return false;
        }
        
        private static void RemoveByClassifyType(this BuffComponent self, int classifyType)
        {
            using (ListComponent<Buff> removeList = ListComponent<Buff>.Create())
            {
                foreach (Entity entity in self.Children.Values)
                {
                    if (entity is Buff buff && buff.Config.Type == classifyType)
                    {
                        removeList.Add(buff);
                    }
                }
                foreach (Buff buff in removeList)
                {
                    self.UnregisterBuff(buff);
                }
            }
        }


        private static Buff FindCoverTarget(this BuffComponent self, int configId, BuffCoverType coverType, BuffCreateInfo buffCreateInfo)
        {
            if (coverType == BuffCoverType.New)
            {
                return null;
            }

            return self.GetByRole(configId, buffCreateInfo.AddUnitId);
        }

        private static void NotifyBuffUpdate(this BuffComponent self, Buff buff)
        {
            Unit owner = buff.Owner;
            if (owner == null || owner.IsDisposed)
            {
                return;
            }

            M2C_BuffUpdate m2CBuffUpdate = M2C_BuffUpdate.Create();
            m2CBuffUpdate.UnitId = owner.Id;
            m2CBuffUpdate.BuffData = buff.ToMessage();
            MapMessageHelper.SendClient(owner, m2CBuffUpdate, (NoticeClientType)buff.Config.NoticeClientType);
        }

        private static void RegisterBuff(this BuffComponent self, Buff buff)
        {
            if (buff == null || buff.IsDisposed)
            {
                return;
            }
            self.BuffsDict.Add(buff.Id,buff);

            Unit owner = buff.Owner;
            if (owner!=null&&!owner.IsDisposed)
            {
                M2C_BuffAdd m2CBuffAdd = M2C_BuffAdd.Create();
                m2CBuffAdd.BuffData = buff.ToMessage();
                m2CBuffAdd.UnitId = owner.Id;
                MapMessageHelper.SendClient(owner,m2CBuffAdd,(NoticeClientType)buff.Config.NoticeClientType);
                //处理buff实体添加具体行为逻辑
                      
                buff.AddActions();//增加buff时行为处理
            }
        }
        

        private static void UnregisterBuff(this BuffComponent self, Buff buff)
        {
            if (buff == null || buff.IsDisposed)
            {
                return;
            }

            try
            {
                
                self.BuffsDict.Remove(buff.Id);
                Unit owner = buff.Owner;
                if (owner!=null&&!owner.IsDisposed)
                {
                    M2C_BuffRemove m2CBuffRemove = M2C_BuffRemove.Create();
                    m2CBuffRemove.BuffId = buff.Id;
                    m2CBuffRemove.UnitId = owner.Id;
                    MapMessageHelper.SendClient(owner,m2CBuffRemove,(NoticeClientType)buff.Config.NoticeClientType);
                    //处理buff实体移除具体行为逻辑
                    buff.RemoveActions();
                    
                }
                buff.Dispose();
            }
            catch (Exception e)
            {
                Log.Error($"Remove Buff {buff.Id} Error!!! {e}");
            }
        }

        private static Buff CreateBuff(this BuffComponent self, BuffCreateInfo buffCreateInfo, Unit owner)
        {
            BuffConfig buffConfig = BuffConfigCategory.Instance.Get(buffCreateInfo.ConfigId);
            Buff buff = self.AddChild<Buff, int>(buffCreateInfo.ConfigId);
            buff.Owner = owner;
            buff.AddUnitId = buffCreateInfo.AddUnitId;
            buff.AddSkillId = buffCreateInfo.AddSkillId;
            buff.Init(self.GetFirstAddLayer(buffCreateInfo,buffConfig));
            self.RegisterBuff(buff);
            return buff;
        }

        private static int GetFirstAddLayer(this BuffComponent self, BuffCreateInfo buffCreateInfo, BuffConfig buffConfig)
        {
            return buffCreateInfo.FirstLayer > 0 ? buffCreateInfo.FirstLayer : buffConfig.FirstAddLayer;
        }

        private static int GetInitialLayer(this BuffComponent self, BuffCreateInfo buffCreateInfo, BuffConfig buffConfig)
        {
            int layer = self.GetFirstAddLayer(buffCreateInfo, buffConfig);
            if (buffConfig.LayerLimit > 0 && layer > buffConfig.LayerLimit)
            {
                return buffConfig.LayerLimit;
            }

            return layer;
        }
    }
}
