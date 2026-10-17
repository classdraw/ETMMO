using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.Options;
using System.ComponentModel;

namespace ET
{
    [Config]
    public partial class RaceConfigCategory : Singleton<RaceConfigCategory>, IMerge
    {
        [BsonElement]
        [BsonDictionaryOptions(DictionaryRepresentation.ArrayOfArrays)]
        private Dictionary<int, RaceConfig> dict = new();
		
        public void Merge(object o)
        {
            RaceConfigCategory s = o as RaceConfigCategory;
            foreach (var kv in s.dict)
            {
                this.dict.Add(kv.Key, kv.Value);
            }
        }
		
        public RaceConfig Get(int id)
        {
            this.dict.TryGetValue(id, out RaceConfig item);

            if (item == null)
            {
                throw new Exception($"配置找不到，配置表名: {nameof (RaceConfig)}，配置id: {id}");
            }

            return item;
        }
		
        public bool Contain(int id)
        {
            return this.dict.ContainsKey(id);
        }

        public Dictionary<int, RaceConfig> GetAll()
        {
            return this.dict;
        }

        public RaceConfig GetOne()
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

	public partial class RaceConfig: ProtoObject, IConfig
	{
		/// <summary>Id</summary>
		public int Id { get; set; }
		/// <summary>类型</summary>
		public int Type { get; set; }
		/// <summary>名字</summary>
		public string Name { get; set; }

	}
}
