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
                    self.RegisterBuff(buff);
                }
            }
        }

        public static BuffCreateInfo Create(this ET.Server.BuffComponent self, int configId, long addUnitId, int addSkillId)
        {
            BuffCreateInfo buffCreateInfo = self.GetComponent<BuffTempComponent>().AddChild<BuffCreateInfo, int>(configId);
            buffCreateInfo.AddUnitId = addUnitId;
            buffCreateInfo.AddSkillId = addSkillId;
            return buffCreateInfo;
        }

        public static bool CreateAddAdd(this ET.Server.BuffComponent self, int configId, long addUnitId, int addSkillId)
        {
            using (BuffCreateInfo buffCreateInfo = self.Create(configId, addUnitId, addSkillId))
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

            BuffCoverHandleResult result = self.HandleBuffCover(buffCreateInfo);
            switch (result)
            {
                case BuffCoverHandleResult.Handled:
                    return true;
                case BuffCoverHandleResult.Rejected:
                    return false;
                case BuffCoverHandleResult.NeedCreate:
                    self.CreateBuff(buffCreateInfo, owner);
                    return true;
                default:
                    return false;
            }
        }

        private static BuffCoverHandleResult HandleBuffCover(this BuffComponent self, BuffCreateInfo buffCreateInfo)
        {
            BuffConfig buffConfig = BuffConfigCategory.Instance.Get(buffCreateInfo.ConfigId);
            int configId = buffCreateInfo.ConfigId;
            int duration = buffConfig.Duration;
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

                    oldBuff.AddDuration(duration);
                    return BuffCoverHandleResult.Handled;
                }
                case BuffCoverType.Replace:
                {
                    if (oldBuff != null)
                    {
                        self.RemoveBuff(oldBuff);
                    }

                    return BuffCoverHandleResult.NeedCreate;
                }
                case BuffCoverType.ResetTime:
                {
                    if (oldBuff == null)
                    {
                        return BuffCoverHandleResult.NeedCreate;
                    }

                    oldBuff.ResetDuration(duration);
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

                    oldBuff.ResetDuration(duration);
                    return BuffCoverHandleResult.Handled;
                }
                case BuffCoverType.ClassifyMutex:
                    self.RemoveByClassifyType(buffConfig.Type);
                    return BuffCoverHandleResult.NeedCreate;
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

            self.RemoveBuff(buff);
        }

        /// <summary>
        /// 按 ConfigId 移除第一个匹配的 Buff（非 New 多实例场景）
        /// </summary>
        public static void RemoveByConfigId(this BuffComponent self, int configId)
        {
            Buff buff = self.GetByConfigId(configId);
            if (buff == null)
            {
                return;
            }

            self.RemoveBuff(buff);
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

        /// <summary>
        /// 按 ConfigId 获取第一个匹配的 Buff
        /// </summary>
        public static Buff GetByConfigId(this BuffComponent self, int configId)
        {
            foreach (EntityRef<Buff> buffRef in self.BuffsDict.Values)
            {
                Buff buff = buffRef;
                if (buff != null && buff.ConfigId == configId)
                {
                    return buff;
                }
            }

            return null;
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

        private static Buff FindCoverTarget(this BuffComponent self, int configId, BuffCoverType coverType, BuffCreateInfo buffCreateInfo)
        {
            return coverType switch
            {
                BuffCoverType.Role => self.GetByRole(configId, buffCreateInfo.AddUnitId),
                BuffCoverType.New => null,
                _ => self.GetByConfigId(configId),
            };
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
                    self.RemoveBuff(buff);
                }
            }
        }

        private static void RegisterBuff(this BuffComponent self, Buff buff)
        {
            self.BuffsDict[buff.Id] = buff;
        }

        private static void UnregisterBuff(this BuffComponent self, Buff buff)
        {
            self.BuffsDict.Remove(buff.Id);
        }

        private static void RemoveBuff(this BuffComponent self, Buff buff)
        {
            if (buff == null || buff.IsDisposed)
            {
                return;
            }

            try
            {
                self.UnregisterBuff(buff);
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
            buff.InitTime(buffConfig.Duration);
            self.RegisterBuff(buff);
            return buff;
        }
    }
}
