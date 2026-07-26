using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.Options;
using System.ComponentModel;

namespace ET
{
    [Config]
    public partial class EffectConfigCategory : Singleton<EffectConfigCategory>, IMerge
    {
        [BsonElement]
        [BsonDictionaryOptions(DictionaryRepresentation.ArrayOfArrays)]
        private Dictionary<int, EffectConfig> dict = new();
		
        public void Merge(object o)
        {
            EffectConfigCategory s = o as EffectConfigCategory;
            foreach (var kv in s.dict)
            {
                this.dict.Add(kv.Key, kv.Value);
            }
        }
		
        public EffectConfig Get(int id)
        {
            this.dict.TryGetValue(id, out EffectConfig item);

            if (item == null)
            {
                throw new Exception($"配置找不到，配置表名: {nameof (EffectConfig)}，配置id: {id}");
            }

            return item;
        }
		
        public bool Contain(int id)
        {
            return this.dict.ContainsKey(id);
        }

        public Dictionary<int, EffectConfig> GetAll()
        {
            return this.dict;
        }

        public EffectConfig GetOne()
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

	public partial class EffectConfig: ProtoObject, IConfig
	{
		/// <summary>Id</summary>
		public int Id { get; set; }
		/// <summary>Type</summary>
		public int Type { get; set; }
		/// <summary>名字</summary>
		public string Title { get; set; }
		/// <summary>模型</summary>
		public string Model { get; set; }
		/// <summary>挂点</summary>
		public int BindBone { get; set; }
		/// <summary>位置偏移</summary>
		public float[] _Offset;
		
		[BsonIgnore]
		public float[] Offset
		{
			get
			{
				if(_Offset == null)
					_Offset = new float[] { };
				return _Offset;
			}
		}
		/// <summary>缩放</summary>
		public float[] _Scale;
		
		[BsonIgnore]
		public float[] Scale
		{
			get
			{
				if(_Scale == null)
					_Scale = new float[] { };
				return _Scale;
			}
		}
		/// <summary>销毁时间(毫秒) -1无限</summary>
		public int DestroyTime { get; set; }

	}
}
