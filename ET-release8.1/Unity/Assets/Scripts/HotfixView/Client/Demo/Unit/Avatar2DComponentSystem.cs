using System;
using System.Collections.Generic;
using UnityEngine;

namespace ET.Client
{
    [FriendOf(typeof(Avatar2DComponent))]
    [EntitySystemOf(typeof(Avatar2DComponent))]
    public static partial class Avatar2DComponentSystem
    {
        
        [EntitySystem]
        private static void Awake(this Avatar2DComponent self,GameObject go)
        {
            if (go == null)
            {
                return;
            }
            
            ReferenceCollector avatarRootCollector = go.GetComponent<ReferenceCollector>();
            if (avatarRootCollector == null)
            {
                return;
            }
            
            self.AvatarObjs.Clear();
            self.AvatarParts.Clear();

            for (int i = 0; i < (int)AvatarType.Count; i++)
            {
                AvatarType avatarType = (AvatarType)i;
                string avatarTypeKey = avatarType.ToString();
                GameObject avatarObj = avatarRootCollector.Get<GameObject>(avatarTypeKey);
                if (avatarObj == null)
                {
                    continue;
                }

                self.AvatarObjs[avatarType] = avatarObj;

                // 3) 每个 AvatarObj 上的 ReferenceCollector（key 为 AvatarPartType）
                ReferenceCollector partsCollector = avatarObj.GetComponent<ReferenceCollector>();
                if (partsCollector == null)
                {
                    continue;
                }

                var partDict = new Dictionary<AvatarPartType, GameObject>();
                for (int j = 0; j < (int)AvatarPartType.Count; j++)
                {
                    AvatarPartType partType = (AvatarPartType)j;
                    string partKey = partType.ToString();
                    GameObject partObj = partsCollector.Get<GameObject>(partKey);
                    if (partObj == null)
                    {
                        continue;
                    }
                    
                    //Log.Info($"Avatar2D Awake: avatarType={avatarType} partType={partType} obj={partObj.name}");
                    partDict[partType] = partObj;
                }

                self.AvatarParts[avatarType] = partDict;
            }
        }
        
        [EntitySystem]
        private static void Destroy(this Avatar2DComponent self)
        {
            self.AvatarObjs?.Clear();

            if (self.AvatarParts != null)
            {
                foreach (KeyValuePair<AvatarType, Dictionary<AvatarPartType, GameObject>> kv in self.AvatarParts)
                {
                    kv.Value?.Clear();
                }

                self.AvatarParts.Clear();
            }
        }
    }
}

