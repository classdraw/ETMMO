using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.Options;
using System.ComponentModel;

namespace ET
{
    [Config]
    public partial class SkillConfigCategory : Singleton<SkillConfigCategory>, IMerge
    {
        [BsonElement]
        [BsonDictionaryOptions(DictionaryRepresentation.ArrayOfArrays)]
        private Dictionary<int, SkillConfig> dict = new();
		
        public void Merge(object o)
        {
            SkillConfigCategory s = o as SkillConfigCategory;
            foreach (var kv in s.dict)
            {
                this.dict.Add(kv.Key, kv.Value);
            }
        }
		
        public SkillConfig Get(int id)
        {
            this.dict.TryGetValue(id, out SkillConfig item);

            if (item == null)
            {
                throw new Exception($"配置找不到，配置表名: {nameof (SkillConfig)}，配置id: {id}");
            }

            return item;
        }
		
        public bool Contain(int id)
        {
            return this.dict.ContainsKey(id);
        }

        public Dictionary<int, SkillConfig> GetAll()
        {
            return this.dict;
        }

        public SkillConfig GetOne()
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

	public partial class SkillConfig: ProtoObject, IConfig
	{
		/// <summary>Id</summary>
		public int Id { get; set; }
		/// <summary>施法表id</summary>
		public int CastId { get; set; }
		/// <summary>名字</summary>
		public string Title { get; set; }
		/// <summary>属性Check</summary>
		public float[] _CheckFunc;
		
		[BsonIgnore]
		public float[] CheckFunc
		{
			get
			{
				if(_CheckFunc == null)
					_CheckFunc = new float[] { };
				return _CheckFunc;
			}
		}
		/// <summary>消耗方法</summary>
		public float[] _ConsumeFunc;
		
		[BsonIgnore]
		public float[] ConsumeFunc
		{
			get
			{
				if(_ConsumeFunc == null)
					_ConsumeFunc = new float[] { };
				return _ConsumeFunc;
			}
		}

	}
}
