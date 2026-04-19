
namespace ET
{
    
    public enum AvatarPartType
    {
        // 粘贴到 ET.Client.AvatarPartType 枚举体内，与现有成员合并；末尾请保留 Count。
        Armor_Left = 0,
        Armor_Right = 1,
        Armor_Body = 2,
        Back = 3,
        Body_Arm_Left = 4,
        Body_Arm_Right = 5,
        Body = 6,
        Cloth_Left = 7,
        Cloth_Right = 8,
        Cloth_Body = 9,
        Eye_Front_Left = 10,
        Eye_Front_Right = 11,
        Eye_Back_Left = 12,
        Eye_Back_Right = 13,
        FaceHair = 14,
        Hair = 15,
        Helmet = 16,
        Foot_Left = 17,
        Foot_Right = 18,
        Shield_Left = 19,
        Shield_Right = 20,
        Weapon_Left = 21,
        Weapon_Right = 22,
        Foot_Cloth_Left = 23,
        Foot_Cloth_Right = 24,
        Shadow = 25,
        Head = 26,
        Count
    }
    
    
    public enum RoleInfoState
    {
        Normal=0,//正常状态
        Freeze=1,//冻结状态
    }
    //角色
    [ChildOf]
    public class RoleInfo:Entity,IAwake
    {
        public string Name;
        public int State;
        public string AccountName;
        public long LastLoginTime;//离线保存的时候做处理
        public long CreateTime;
        public int ServerId;
        
        
        //avatar数据
        public int BaseAvatar;//保存基础avatar组，角色皮肤，角色穿着在unit存储

    }
}
