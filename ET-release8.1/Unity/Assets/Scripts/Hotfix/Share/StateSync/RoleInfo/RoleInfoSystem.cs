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

        public static void FromMessage(this RoleInfo self, RoleInfoProto roleInfoProto)
        {
            self.Name = roleInfoProto.Name;
            self.State = roleInfoProto.State;
            self.AccountName = roleInfoProto.AccountName;
            self.LastLoginTime = roleInfoProto.LastLoginTime;
            self.CreateTime = roleInfoProto.CreateTime;
            self.ServerId = roleInfoProto.ServerId;
            self.BaseExternalDisplay = roleInfoProto.BaseExternalDisplay;
        }

        public static RoleInfoProto ToMessage(this RoleInfo self)
        {
            RoleInfoProto roleInfoProto = RoleInfoProto.Create();
            roleInfoProto.Id = self.Id;
            roleInfoProto.Name = self.Name;
            roleInfoProto.State = self.State;
            roleInfoProto.AccountName = self.AccountName;
            roleInfoProto.LastLoginTime = self.LastLoginTime;
            roleInfoProto.CreateTime = self.CreateTime;
            roleInfoProto.ServerId = self.ServerId;
            roleInfoProto.BaseExternalDisplay = self.BaseExternalDisplay;

            return roleInfoProto;
        }
    }
}
