using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.Options;
using System.ComponentModel;

namespace ET
{
    [Config]
    public partial class ExternalDisplayConfigCategory : Singleton<ExternalDisplayConfigCategory>, IMerge
    {
        [BsonElement]
        [BsonDictionaryOptions(DictionaryRepresentation.ArrayOfArrays)]
        private Dictionary<int, ExternalDisplayConfig> dict = new();
		
        public void Merge(object o)
        {
            ExternalDisplayConfigCategory s = o as ExternalDisplayConfigCategory;
            foreach (var kv in s.dict)
            {
                this.dict.Add(kv.Key, kv.Value);
            }
        }
		
        public ExternalDisplayConfig Get(int id)
        {
            this.dict.TryGetValue(id, out ExternalDisplayConfig item);

            if (item == null)
            {
                throw new Exception($"配置找不到，配置表名: {nameof (ExternalDisplayConfig)}，配置id: {id}");
            }

            return item;
        }
		
        public bool Contain(int id)
        {
            return this.dict.ContainsKey(id);
        }

        public Dictionary<int, ExternalDisplayConfig> GetAll()
        {
            return this.dict;
        }

        public ExternalDisplayConfig GetOne()
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

	public partial class ExternalDisplayConfig: ProtoObject, IConfig
	{
		/// <summary>Id</summary>
		public int Id { get; set; }
		/// <summary>显示配表Id</summary>
		public int DisplayId { get; set; }
		/// <summary>性别</summary>
		public int Gender { get; set; }
		/// <summary>种族</summary>
		public int Race { get; set; }
		/// <summary>名字</summary>
		public string Name { get; set; }
		/// <summary>体型</summary>
		public int BodyType { get; set; }
		/// <summary>是否看身材体型</summary>
		public int NeedBodyType { get; set; }
		/// <summary>描述</summary>
		public string Desc { get; set; }

	}
}
