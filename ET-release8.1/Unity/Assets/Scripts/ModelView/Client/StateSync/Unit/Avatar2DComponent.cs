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
    
    [ComponentOf(typeof(Unit))]
    public class Avatar2DComponent : Entity, IAwake<GameObject>, IDestroy
    {
        public Dictionary<AvatarType, GameObject> AvatarObjs = new Dictionary<AvatarType, GameObject>();

        public Dictionary<AvatarType, Dictionary<AvatarPartType, SpriteRenderer>> AvatarParts =
                new Dictionary<AvatarType, Dictionary<AvatarPartType, SpriteRenderer>>();
    }
}
