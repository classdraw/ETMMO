using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.Options;

namespace ET
{
    [FriendOf(typeof (NumericComponent))]
    public static class NumericComponentSystem
    {
        public static float GetAsFloat(this NumericComponent self, int numericType)
        {
            return (float)self.GetByKey(numericType) / 10000;
        }

        public static int GetAsInt(this NumericComponent self, int numericType)
        {
            return (int)self.GetByKey(numericType);
        }

        public static long GetAsLong(this NumericComponent self, int numericType)
        {
            return self.GetByKey(numericType);
        }

        public static void Set(this NumericComponent self, int nt, float value)
        {
            self[nt] = (long)(value * 10000);
        }

        public static void Set(this NumericComponent self, int nt, int value)
        {
            self[nt] = value;
        }

        public static void Set(this NumericComponent self, int nt, long value)
        {
            self[nt] = value;
        }

        public static void SetNoEvent(this NumericComponent self, int numericType, long value)
        {
            self.Insert(numericType, value, false);
        }

        public static void Insert(this NumericComponent self, int numericType, long value, bool isPublicEvent = true)
        {
            long oldValue = self.GetByKey(numericType);
            if (oldValue == value)
            {
                return;
            }

            self.NumericDic[numericType] = value;

            if (NumericType.IsComponentKey(numericType))
            {
                self.Update(numericType, isPublicEvent);
                return;
            }

            if (isPublicEvent)
            {
                EventSystem.Instance.Publish(self.Scene(),
                    new NumbericChange() { Unit = self.GetParent<Unit>(), New = value, Old = oldValue, NumericType = numericType });
            }
        }

        public static long GetByKey(this NumericComponent self, int key)
        {
            long value = 0;
            self.NumericDic.TryGetValue(key, out value);
            return value;
        }

        public static void Update(this NumericComponent self, int numericType, bool isPublicEvent)
        {
            int final = NumericType.ToFinalNumericType(numericType);
            int bas = NumericType.ToComponentKey(final, NumericType.ComponentIndexBase);
            int add = NumericType.ToComponentKey(final, NumericType.ComponentIndexAdd);
            int pct = NumericType.ToComponentKey(final, NumericType.ComponentIndexPct);
            int finalAdd = NumericType.ToComponentKey(final, NumericType.ComponentIndexFinalAdd);
            int finalPct = NumericType.ToComponentKey(final, NumericType.ComponentIndexFinalPct);
            int tempAdd = NumericType.ToComponentKey(final, NumericType.ComponentIndexTempAdd);
            int tempPct = NumericType.ToComponentKey(final, NumericType.ComponentIndexTempPct);
            int tempFinalAdd = NumericType.ToComponentKey(final, NumericType.ComponentIndexTempFinalAdd);
            int tempFinalPct = NumericType.ToComponentKey(final, NumericType.ComponentIndexTempFinalPct);

            long addSum = self.GetByKey(add) + self.GetByKey(tempAdd);
            float pctSum = self.GetAsFloat(pct) + self.GetAsFloat(tempPct);
            long finalAddSum = self.GetByKey(finalAdd) + self.GetByKey(tempFinalAdd);
            float finalPctSum = self.GetAsFloat(finalPct) + self.GetAsFloat(tempFinalPct);

            // final = (((base + add + tempAdd) * (100 + pct + tempPct) / 100) + finalAdd + tempFinalAdd) * (100 + finalPct + tempFinalPct) / 100;
            long result = (long)(((self.GetByKey(bas) + addSum) * (100 + pctSum) / 100f + finalAddSum) * (100 + finalPctSum) / 100f);
            self.Insert(final, result, isPublicEvent);
        }

        /// <summary>登录等场景：清空所有 Temp 分量（+6~+9），并触发对应 Final 重算。</summary>
        public static bool ResetAllTempComponents(this NumericComponent self)
        {
            if (self.NumericDic == null || self.NumericDic.Count == 0)
            {
                return false;
            }

            bool changed = false;
            int[] keys = new int[self.NumericDic.Count];
            self.NumericDic.Keys.CopyTo(keys, 0);
            foreach (int key in keys)
            {
                if (!NumericType.IsTempComponentKey(key) || self.GetByKey(key) == 0)
                {
                    continue;
                }

                self.SetNoEvent(key, 0);
                changed = true;
            }

            return changed;
        }
    }
    
    public struct NumbericChange
    {
        public Unit Unit;
        public int NumericType;
        public long Old;
        public long New;
    }

    [ComponentOf(typeof (Unit))]
    public class NumericComponent: Entity, IAwake, ITransfer,IUnitCache
    {
        [BsonDictionaryOptions(DictionaryRepresentation.ArrayOfArrays)]
        public Dictionary<int, long> NumericDic = new Dictionary<int, long>();

        public long this[int numericType]
        {
            get
            {
                return this.GetByKey(numericType);
            }
            set
            {
                this.Insert(numericType, value);
            }
        }
    }
}