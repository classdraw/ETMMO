using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.Options;
using System.ComponentModel;

namespace ET
{
    [Config]
    public partial class AvatarConfigCategory : Singleton<AvatarConfigCategory>, IMerge
    {
        [BsonElement]
        [BsonDictionaryOptions(DictionaryRepresentation.ArrayOfArrays)]
        private Dictionary<int, AvatarConfig> dict = new();
		
        public void Merge(object o)
        {
            AvatarConfigCategory s = o as AvatarConfigCategory;
            foreach (var kv in s.dict)
            {
                this.dict.Add(kv.Key, kv.Value);
            }
        }
		
        public AvatarConfig Get(int id)
        {
            this.dict.TryGetValue(id, out AvatarConfig item);

            if (item == null)
            {
                throw new Exception($"配置找不到，配置表名: {nameof (AvatarConfig)}，配置id: {id}");
            }

            return item;
        }
		
        public bool Contain(int id)
        {
            return this.dict.ContainsKey(id);
        }

        public Dictionary<int, AvatarConfig> GetAll()
        {
            return this.dict;
        }

        public AvatarConfig GetOne()
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

	public partial class AvatarConfig: ProtoObject, IConfig
	{
		/// <summary>Id</summary>
		public int Id { get; set; }
		/// <summary>Type</summary>
		public int AvatarType { get; set; }
		/// <summary>名字</summary>
		public string Name { get; set; }
		/// <summary>模型</summary>
		public string Model { get; set; }

	}
}
