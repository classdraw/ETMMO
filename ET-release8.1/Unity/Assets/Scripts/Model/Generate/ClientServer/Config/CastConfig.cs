using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.Options;
using System.ComponentModel;

namespace ET
{
    [Config]
    public partial class CastConfigCategory : Singleton<CastConfigCategory>, IMerge
    {
        [BsonElement]
        [BsonDictionaryOptions(DictionaryRepresentation.ArrayOfArrays)]
        private Dictionary<int, CastConfig> dict = new();
		
        public void Merge(object o)
        {
            CastConfigCategory s = o as CastConfigCategory;
            foreach (var kv in s.dict)
            {
                this.dict.Add(kv.Key, kv.Value);
            }
        }
		
        public CastConfig Get(int id)
        {
            this.dict.TryGetValue(id, out CastConfig item);

            if (item == null)
            {
                throw new Exception($"配置找不到，配置表名: {nameof (CastConfig)}，配置id: {id}");
            }

            return item;
        }
		
        public bool Contain(int id)
        {
            return this.dict.ContainsKey(id);
        }

        public Dictionary<int, CastConfig> GetAll()
        {
            return this.dict;
        }

        public CastConfig GetOne()
        {
            if (this.dict == null || this.dict.Count <= 0)
            {
                return null;
            }
            
            var enumerator = this.dict.Values.GetEnumerator();
            enumerator.MoveNext();
            return enumerator.Current; 
        }
    }

	public partial class CastConfig: ProtoObject, IConfig
	{
		/// <summary>Id</summary>
		public int Id { get; set; }
		/// <summary>Type</summary>
		public int Type { get; set; }
		/// <summary>名字</summary>
		public string Name { get; set; }
		/// <summary>目标选择方式</summary>
		public int SelectType { get; set; }
		/// <summary>形状</summary>
		public int[] SelectParam { get; set; }
		/// <summary>形状</summary>
		public int[] ShapeParam { get; set; }
		/// <summary>通知客户端类型</summary>
		public int NoticeClientType { get; set; }
		/// <summary>命中行为</summary>
		public int[] HitAction { get; set; }
		/// <summary>技能命中目标时间点</summary>
		public int[] HitActionTimes { get; set; }
		/// <summary>命中自身行为</summary>
		public int[] SelfHitAction { get; set; }
		/// <summary>技能命中自身时间点</summary>
		public int[] SelfHitActionTimes { get; set; }
		/// <summary>命中Buff</summary>
		public int[] HitBuffs { get; set; }
		/// <summary>命中自身Buff</summary>
		public int[] SelfHitBuffs { get; set; }
		/// <summary>技能不可打断时间</summary>
		public int UnBreakTime { get; set; }
		/// <summary>技能总时长</summary>
		public int TotalTime { get; set; }
		/// <summary>技能开始特效</summary>
		public int[] StartEffect { get; set; }
		/// <summary>技能命中特效</summary>
		public int[] SelfHitEffect { get; set; }
		/// <summary>技能命中特效</summary>
		public int[] HitEffect { get; set; }
		/// <summary>施法转向</summary>
		public bool NeedLookTarget { get; set; }

	}
}
