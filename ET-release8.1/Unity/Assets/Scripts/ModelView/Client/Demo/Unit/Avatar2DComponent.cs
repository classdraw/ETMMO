using System.Collections.Generic;
using UnityEngine;

namespace ET.Client
{

    public enum AvatarType
    {
        Unit=0,
        Horse,
        Count
    }

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
        Count
    }
    [ComponentOf(typeof(Unit))]
    public class Avatar2DComponent : Entity, IAwake<GameObject>, IDestroy
    {
        public Dictionary<AvatarType, GameObject> AvatarObjs = new Dictionary<AvatarType, GameObject>();

        public Dictionary<AvatarType, Dictionary<AvatarPartType, SpriteRenderer>> AvatarParts =
                new Dictionary<AvatarType, Dictionary<AvatarPartType, SpriteRenderer>>();
    }
}
