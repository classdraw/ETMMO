using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.Options;
using System.ComponentModel;

namespace ET
{
    [Config]
    public partial class UnitConfigCategory : Singleton<UnitConfigCategory>, IMerge
    {
        [BsonElement]
        [BsonDictionaryOptions(DictionaryRepresentation.ArrayOfArrays)]
        private Dictionary<int, UnitConfig> dict = new();
		
        public void Merge(object o)
        {
            UnitConfigCategory s = o as UnitConfigCategory;
            foreach (var kv in s.dict)
            {
                this.dict.Add(kv.Key, kv.Value);
            }
        }
		
        public UnitConfig Get(int id)
        {
            this.dict.TryGetValue(id, out UnitConfig item);

            if (item == null)
            {
                throw new Exception($"配置找不到，配置表名: {nameof (UnitConfig)}，配置id: {id}");
            }

            return item;
        }
		
        public bool Contain(int id)
        {
            return this.dict.ContainsKey(id);
        }

        public Dictionary<int, UnitConfig> GetAll()
        {
            return this.dict;
        }

        public UnitConfig GetOne()
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

	public partial class UnitConfig: ProtoObject, IConfig
	{
		/// <summary>Id</summary>
		public int Id { get; set; }
		/// <summary>Type</summary>
		public int Type { get; set; }
		/// <summary>名字</summary>
		public string Name { get; set; }
		/// <summary>位置</summary>
		public int Position { get; set; }
		/// <summary>速度*1000</summary>
		public int Speed { get; set; }
		/// <summary>AOI</summary>
		public int Aoi { get; set; }
		/// <summary>血量Job系数*1000</summary>
		public int JobHp { get; set; }
		/// <summary>蓝量Job系数*1000</summary>
		public double JobSp { get; set; }
		/// <summary>STR</summary>
		public int Str { get; set; }
		/// <summary>AGI</summary>
		public int Agi { get; set; }
		/// <summary>VIT</summary>
		public int Vit { get; set; }
		/// <summary>INT</summary>
		public int Intell { get; set; }
		/// <summary>DEX</summary>
		public int Dex { get; set; }
		/// <summary>LUK</summary>
		public int Luk { get; set; }

	}
}
