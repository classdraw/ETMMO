using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.Options;
using System.ComponentModel;

namespace ET
{
    [Config]
    public partial class MapConfigCategory : Singleton<MapConfigCategory>, IMerge
    {
        [BsonElement]
        [BsonDictionaryOptions(DictionaryRepresentation.ArrayOfArrays)]
        private Dictionary<int, MapConfig> dict = new();
		
        public void Merge(object o)
        {
            MapConfigCategory s = o as MapConfigCategory;
            foreach (var kv in s.dict)
            {
                this.dict.Add(kv.Key, kv.Value);
            }
        }
		
        public MapConfig Get(int id)
        {
            this.dict.TryGetValue(id, out MapConfig item);

            if (item == null)
            {
                throw new Exception($"配置找不到，配置表名: {nameof (MapConfig)}，配置id: {id}");
            }

            return item;
        }
		
        public bool Contain(int id)
        {
            return this.dict.ContainsKey(id);
        }

        public Dictionary<int, MapConfig> GetAll()
        {
            return this.dict;
        }

        public MapConfig GetOne()
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

	public partial class MapConfig: ProtoObject, IConfig
	{
		/// <summary>Id</summary>
		public int Id { get; set; }
		/// <summary>Type</summary>
		public int Type { get; set; }
		/// <summary>名字</summary>
		public string Title { get; set; }
		/// <summary>地图逻辑名</summary>
		public string LogicName { get; set; }
		/// <summary>最大人数</summary>
		public int MaxPlayer { get; set; }
		/// <summary>是否自动创建</summary>
		public bool AutoCreate { get; set; }
		/// <summary>是否分线</summary>
		public bool Div { get; set; }
		/// <summary>有效时间（秒）</summary>
		public int ValidTime { get; set; }
		/// <summary>初始点</summary>
		public float[] _StartPoint;
		
		[BsonIgnore]
		public float[] StartPoint
		{
			get
			{
				if(_StartPoint == null)
					_StartPoint = new float[] { };
				return _StartPoint;
			}
		}
		/// <summary>地图脚本</summary>
		public string[] _CmdList;
		
		[BsonIgnore]
		public string[] CmdList
		{
			get
			{
				if(_CmdList == null)
					_CmdList = new string[] { };
				return _CmdList;
			}
		}

	}
}
