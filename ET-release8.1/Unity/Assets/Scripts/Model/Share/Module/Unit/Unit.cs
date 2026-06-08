using System.Diagnostics;
using MongoDB.Bson.Serialization.Attributes;
using Unity.Mathematics;

namespace ET
{
    [ChildOf(typeof(UnitComponent))]
    [DebuggerDisplay("ViewName,nq")]
    public partial class Unit: Entity, IAwake<int,string>,IGetComponentSys
    {
        public int ConfigId { get; set; } //配置表id
        public string Name { get; set; } //角色名字
        [BsonElement]
        private float3 position; //坐标

        [BsonIgnore]
        public float3 Position
        {
            get => this.position;
            set
            {
                float3 oldPos = this.position;
                this.position = value;
                EventSystem.Instance.Publish(this.Scene(), new ChangePosition() { Unit = this, OldPos = oldPos });
            }
        }

        [BsonIgnore]
        public float3 Forward
        {
            get => math.mul(this.Rotation, math.forward());
            set => this.Rotation = quaternion.LookRotation(value, math.up());
        }
        
        [BsonElement]
        private quaternion rotation;
        
        [BsonIgnore]
        public quaternion Rotation
        {
            get => this.rotation;
            set
            {
                this.rotation = value;
                EventSystem.Instance.Publish(this.Scene(), new ChangeRotation() { Unit = this });
            }
        }
        
        /// <summary>
        /// 当前所在地图
        /// </summary>
        public int MapId { get; set; }

        /// <summary>
        /// 上次离开的地图
        /// </summary>
        public int LastMapId { get; set; }

        public int MapUid { get; set; }
        /// <summary>
        /// 队伍 Id；0 表示无队伍，同一非 0 值为盟友（自由PK）
        /// </summary>
        public long TeamId { get; set; }
        /// <summary>
        /// 主人 UnitId；召唤物/宠物指向主人 Unit，其余为 0
        /// </summary>
        [BsonIgnore]
        public long OwnerId { get; set; }



        protected override string ViewName
        {
            get
            {
                return $"{this.GetType().FullName} ({this.Id})";
            }
        }
    }
}