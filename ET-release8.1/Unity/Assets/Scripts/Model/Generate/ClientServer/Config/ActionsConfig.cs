using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.Options;
using System.ComponentModel;

namespace ET
{
    [Config]
    public partial class ActionsConfigCategory : Singleton<ActionsConfigCategory>, IMerge
    {
        [BsonElement]
        [BsonDictionaryOptions(DictionaryRepresentation.ArrayOfArrays)]
        private Dictionary<int, ActionsConfig> dict = new();
		
        public void Merge(object o)
        {
            ActionsConfigCategory s = o as ActionsConfigCategory;
            foreach (var kv in s.dict)
            {
                this.dict.Add(kv.Key, kv.Value);
            }
        }
		
        public ActionsConfig Get(int id)
        {
            this.dict.TryGetValue(id, out ActionsConfig item);

            if (item == null)
            {
                throw new Exception($"配置找不到，配置表名: {nameof (ActionsConfig)}，配置id: {id}");
            }

            return item;
        }
		
        public bool Contain(int id)
        {
            return this.dict.ContainsKey(id);
        }

        public Dictionary<int, ActionsConfig> GetAll()
        {
            return this.dict;
        }

        public ActionsConfig GetOne()
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

	public partial class ActionsConfig: ProtoObject, IConfig
	{
		/// <summary>Id</summary>
		public int Id { get; set; }
		/// <summary>技能行为类型</summary>
		public int Type { get; set; }

	}
}
