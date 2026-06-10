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
		/// <summary>目标选择参数</summary>
		public int[] SelectParam { get; set; }
		/// <summary>通知客户端类型</summary>
		public int NoticeClientType { get; set; }
		/// <summary>技能命中目标时间点</summary>
		public int[] HitActionTimes { get; set; }
		/// <summary>技能命中自身时间点</summary>
		public int[] SelfHitActionTimes { get; set; }

	}
}
