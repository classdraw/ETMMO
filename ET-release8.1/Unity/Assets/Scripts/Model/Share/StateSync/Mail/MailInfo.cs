using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;

namespace ET
{
    [ChildOf]
    public class MailInfo: Entity,IAwake,IDestroy,ISerializeToEntity,IDeserialize
    {
        public int ConfigId { get; set; } //前端配置表id 有这个默认读这个
        public string Title { get; set; } //标题 可能没有
        public string Message { get; set; } //内容 可能没有

        public bool IsRead { get; set; }//是否已读
        public bool IsCollected { get; set; } //是否已经领取附件
        
        
        [BsonIgnore]
        public List<EntityRef<Item>> RewardList = new List<EntityRef<Item>>();
    }
}
