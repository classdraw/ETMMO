using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.Options;
using System.ComponentModel;

namespace ET
{
    [Config]
    public partial class ExpConfigCategory : Singleton<ExpConfigCategory>, IMerge
    {
        [BsonElement]
        [BsonDictionaryOptions(DictionaryRepresentation.ArrayOfArrays)]
        private Dictionary<int, ExpConfig> dict = new();
		
        public void Merge(object o)
        {
            ExpConfigCategory s = o as ExpConfigCategory;
            foreach (var kv in s.dict)
            {
                this.dict.Add(kv.Key, kv.Value);
            }
        }
		
        public ExpConfig Get(int id)
        {
            this.dict.TryGetValue(id, out ExpConfig item);

            if (item == null)
            {
                throw new Exception($"配置找不到，配置表名: {nameof (ExpConfig)}，配置id: {id}");
            }

            return item;
        }
		
        public bool Contain(int id)
        {
            return this.dict.ContainsKey(id);
        }

        public Dictionary<int, ExpConfig> GetAll()
        {
            return this.dict;
        }

        public ExpConfig GetOne()
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

	public partial class ExpConfig: ProtoObject, IConfig
	{
		/// <summary>Id</summary>
		public int Id { get; set; }
		/// <summary>需要经验</summary>
		public int NeedExp { get; set; }

	}
}
