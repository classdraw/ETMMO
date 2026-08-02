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
		public int[] _SelectParam;
		
		[BsonIgnore]
		public int[] SelectParam
		{
			get
			{
				if(_SelectParam == null)
					_SelectParam = new int[] { };
				return _SelectParam;
			}
		}
		/// <summary>形状</summary>
		public int[] _ShapeParam;
		
		[BsonIgnore]
		public int[] ShapeParam
		{
			get
			{
				if(_ShapeParam == null)
					_ShapeParam = new int[] { };
				return _ShapeParam;
			}
		}
		/// <summary>通知客户端类型</summary>
		public int NoticeClientType { get; set; }
		/// <summary>命中行为</summary>
		public int[] _HitAction;
		
		[BsonIgnore]
		public int[] HitAction
		{
			get
			{
				if(_HitAction == null)
					_HitAction = new int[] { };
				return _HitAction;
			}
		}
		/// <summary>技能命中目标时间点</summary>
		public int[] _HitActionTimes;
		
		[BsonIgnore]
		public int[] HitActionTimes
		{
			get
			{
				if(_HitActionTimes == null)
					_HitActionTimes = new int[] { };
				return _HitActionTimes;
			}
		}
		/// <summary>命中自身行为</summary>
		public int[] _SelfHitAction;
		
		[BsonIgnore]
		public int[] SelfHitAction
		{
			get
			{
				if(_SelfHitAction == null)
					_SelfHitAction = new int[] { };
				return _SelfHitAction;
			}
		}
		/// <summary>技能命中自身时间点</summary>
		public int[] _SelfHitActionTimes;
		
		[BsonIgnore]
		public int[] SelfHitActionTimes
		{
			get
			{
				if(_SelfHitActionTimes == null)
					_SelfHitActionTimes = new int[] { };
				return _SelfHitActionTimes;
			}
		}
		/// <summary>命中Buff</summary>
		public int[] _HitBuffs;
		
		[BsonIgnore]
		public int[] HitBuffs
		{
			get
			{
				if(_HitBuffs == null)
					_HitBuffs = new int[] { };
				return _HitBuffs;
			}
		}
		/// <summary>命中自身Buff</summary>
		public int[] _SelfHitBuffs;
		
		[BsonIgnore]
		public int[] SelfHitBuffs
		{
			get
			{
				if(_SelfHitBuffs == null)
					_SelfHitBuffs = new int[] { };
				return _SelfHitBuffs;
			}
		}
		/// <summary>技能不可打断时间</summary>
		public int UnBreakTime { get; set; }
		/// <summary>技能总时长</summary>
		public int TotalTime { get; set; }
		/// <summary>技能冷却</summary>
		public int CoolDown { get; set; }
		/// <summary>技能开始特效</summary>
		public int[] _StartEffect;
		
		[BsonIgnore]
		public int[] StartEffect
		{
			get
			{
				if(_StartEffect == null)
					_StartEffect = new int[] { };
				return _StartEffect;
			}
		}
		/// <summary>技能命中特效</summary>
		public int[] _SelfHitEffect;
		
		[BsonIgnore]
		public int[] SelfHitEffect
		{
			get
			{
				if(_SelfHitEffect == null)
					_SelfHitEffect = new int[] { };
				return _SelfHitEffect;
			}
		}
		/// <summary>技能命中特效</summary>
		public int[] _HitEffect;
		
		[BsonIgnore]
		public int[] HitEffect
		{
			get
			{
				if(_HitEffect == null)
					_HitEffect = new int[] { };
				return _HitEffect;
			}
		}
		/// <summary>施法转向</summary>
		public bool NeedLookTarget { get; set; }

	}
}
