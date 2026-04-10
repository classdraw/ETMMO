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
        Back=0, 
        Back_2, 
        Body, 
        BodyArmor, 
        ClothBody, 
        Front, 
        Front_2, 
        LWeapon, 
        R11Helmet1, 
        R11RCloth, 
        R12RFoot, 
        R20LArm, 
        R20RArm, 
        R2LCloth, 
        R3LFoot, 
        R5Head, 
        R6FaceHair, 
        R7Hair, 
        RWeapon, 
        Shadow,
        Count
        
    }
    [ComponentOf(typeof(Unit))]
    public class Avatar2DComponent : Entity, IAwake<GameObject>, IDestroy
    {
        public Dictionary<AvatarType, GameObject> AvatarObjs = new Dictionary<AvatarType, GameObject>();

        public Dictionary<AvatarType, Dictionary<AvatarPartType, GameObject>> AvatarParts =
                new Dictionary<AvatarType, Dictionary<AvatarPartType, GameObject>>();
    }
}
