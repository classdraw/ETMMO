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
            
            return roleInfoProto;
        }
    }
}

