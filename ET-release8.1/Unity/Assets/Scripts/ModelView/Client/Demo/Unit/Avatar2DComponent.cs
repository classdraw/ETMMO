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
        Armor_Left=0,
        Armor_Right,
        Armor_Body,
        Back,
        Body_Arm_Left,
        Body_Arm_Right,
        Body,
        Cloth_Left,
        Cloth_Right,
        Cloth_Body,
        Eye_Front_Left,
        Eye_Front_Right,
        Eye_Back_Left,
        Eye_Back_Right,
        FaceHair,
        Hair,
        Helmet,
        Foot_Left,
        Foot_Right,
        Shield_Left,
        Shield_Right,
        Weapon_Left,
        Weapon_Right,
        Foot_Cloth_Left,
        Foot_Cloth_Right,
        Shadow,
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
