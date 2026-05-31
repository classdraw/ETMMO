using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.Options;
using System.ComponentModel;

namespace ET
{
    [Config]
    public partial class MailConfigCategory : Singleton<MailConfigCategory>, IMerge
    {
        [BsonElement]
        [BsonDictionaryOptions(DictionaryRepresentation.ArrayOfArrays)]
        private Dictionary<int, MailConfig> dict = new();
		
        public void Merge(object o)
        {
            MailConfigCategory s = o as MailConfigCategory;
            foreach (var kv in s.dict)
            {
                this.dict.Add(kv.Key, kv.Value);
            }
        }
		
        public MailConfig Get(int id)
        {
            this.dict.TryGetValue(id, out MailConfig item);

            if (item == null)
            {
                throw new Exception($"配置找不到，配置表名: {nameof (MailConfig)}，配置id: {id}");
            }

            return item;
        }
		
        public bool Contain(int id)
        {
            return this.dict.ContainsKey(id);
        }

        public Dictionary<int, MailConfig> GetAll()
        {
            return this.dict;
        }

        public MailConfig GetOne()
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

	public partial class MailConfig: ProtoObject, IConfig
	{
		/// <summary>Id</summary>
		public int Id { get; set; }
		/// <summary>Type</summary>
		public int Type { get; set; }
		/// <summary>名字</summary>
		public string Title { get; set; }
		/// <summary>信息</summary>
		public string Message { get; set; }
		/// <summary>奖励</summary>
		public int[] RewardArray { get; set; }

	}
}
