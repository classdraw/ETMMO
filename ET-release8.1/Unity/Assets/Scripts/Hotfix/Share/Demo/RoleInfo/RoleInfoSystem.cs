namespace ET
{
    [EntitySystemOf(typeof(RoleInfo))]
    [FriendOf(typeof(RoleInfo))]
    public static partial class RoleInfoSystem
    {
        [EntitySystem]
        private static void Awake(this RoleInfo self)
        {
            
        }

        public static void FromMessage(this RoleInfo self,RoleInfoProto roleInfoProto)
        {
            //这里不需要id 因为entity有自己的id逻辑
            self.Name = roleInfoProto.Name;
            self.State = roleInfoProto.State;
            self.AccountName = roleInfoProto.AccountName;
            self.LastLoginTime = roleInfoProto.LastLoginTime;
            self.CreateTime = roleInfoProto.CreateTime;
            self.ServerId = roleInfoProto.ServerId;
            self.ArmorBody = roleInfoProto.ArmorBody;
            self.ArmorLeft = roleInfoProto.ArmorLeft;
            self.ArmorRight = roleInfoProto.ArmorRight;
            self.Body = roleInfoProto.Body;
            self.BodyArmLeft = roleInfoProto.BodyArmLeft;
            self.BodyArmRight = roleInfoProto.BodyArmRight;
            self.FootLeft = roleInfoProto.FootLeft;
            self.FootRight = roleInfoProto.FootRight;
            self.Head = roleInfoProto.Head;
            self.EyeFront = roleInfoProto.EyeFront;
            self.EyeBack = roleInfoProto.EyeBack;
            self.Hair = roleInfoProto.Hair;
        }

        public static RoleInfoProto ToMessage(this RoleInfo self)
        {
            var roleInfoProto = RoleInfoProto.Create();
            roleInfoProto.Id = self.Id;
            roleInfoProto.Name = self.Name;
            roleInfoProto.State = self.State;
            roleInfoProto.AccountName = self.AccountName;
            roleInfoProto.LastLoginTime = self.LastLoginTime;
            roleInfoProto.CreateTime = self.CreateTime;
            roleInfoProto.ServerId = self.ServerId;
            roleInfoProto.ArmorBody = self.ArmorBody;
            roleInfoProto.ArmorLeft = self.ArmorLeft;
            roleInfoProto.ArmorRight = self.ArmorRight;
            roleInfoProto.Body = self.Body;
            roleInfoProto.BodyArmLeft = self.BodyArmLeft;
            roleInfoProto.BodyArmRight = self.BodyArmRight;
            roleInfoProto.FootLeft = self.FootLeft;
            roleInfoProto.FootRight = self.FootRight;
            roleInfoProto.Head = self.Head;
            roleInfoProto.EyeFront = self.EyeFront;
            roleInfoProto.EyeBack = self.EyeBack;
            roleInfoProto.Hair = self.Hair;

            return roleInfoProto;
        }
    }
}

