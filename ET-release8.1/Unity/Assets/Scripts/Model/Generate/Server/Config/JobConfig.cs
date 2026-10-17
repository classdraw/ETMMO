using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.Options;
using System.ComponentModel;

namespace ET
{
    [Config]
    public partial class JobConfigCategory : Singleton<JobConfigCategory>, IMerge
    {
        [BsonElement]
        [BsonDictionaryOptions(DictionaryRepresentation.ArrayOfArrays)]
        private Dictionary<int, JobConfig> dict = new();
		
        public void Merge(object o)
        {
            JobConfigCategory s = o as JobConfigCategory;
            foreach (var kv in s.dict)
            {
                this.dict.Add(kv.Key, kv.Value);
            }
        }
		
        public JobConfig Get(int id)
        {
            this.dict.TryGetValue(id, out JobConfig item);

            if (item == null)
            {
                throw new Exception($"配置找不到，配置表名: {nameof (JobConfig)}，配置id: {id}");
            }

            return item;
        }
		
        public bool Contain(int id)
        {
            return this.dict.ContainsKey(id);
        }

        public Dictionary<int, JobConfig> GetAll()
        {
            return this.dict;
        }

        public JobConfig GetOne()
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

	public partial class JobConfig: ProtoObject, IConfig
	{
		/// <summary>Id</summary>
		public int Id { get; set; }
		/// <summary>对应Unit实体</summary>
		public int UnitId { get; set; }
		/// <summary>类型</summary>
		public int Type { get; set; }
		/// <summary>名字</summary>
		public string Name { get; set; }
		/// <summary>Model</summary>
		public string Model { get; set; }

	}
}
