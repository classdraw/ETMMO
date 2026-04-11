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

                var partDict = new Dictionary<AvatarPartType, SpriteRenderer>();
                for (int j = 0; j < (int)AvatarPartType.Count; j++)
                {
                    AvatarPartType partType = (AvatarPartType)j;
                    SpriteRenderer partObj = GetPartSpriteRenderer(partsCollector, partType);
                    if (partObj == null)
                    {
                        continue;
                    }
                    
                    //Log.Info($"Avatar2D Awake: avatarType={avatarType} partType={partType} obj={partObj.name}");
                    partDict[partType] = partObj;
                }

                self.AvatarParts[avatarType] = partDict;
            }
            
            
            self.AvatarObjs[AvatarType.Horse].SetActive(false);
            self.AvatarObjs[AvatarType.Unit].SetActive(true);
        }

        /// <summary>
        /// 先按枚举名（如 EyeBack）取绑点；若无则尝试 ReferenceCollector 收集用的下划线左右 key（与编辑器白名单一致）。
        /// 若左右两个都存在，优先 Left（单槽位枚举仅保留一个引用）。
        /// </summary>
        private static SpriteRenderer GetPartSpriteRenderer(ReferenceCollector partsCollector, AvatarPartType partType)
        {
            string primary = partType.ToString();
            SpriteRenderer r = partsCollector.Get<SpriteRenderer>(primary);
            if (r != null)
            {
                return r;
            }
            Log.Error("GetPartSpriteRenderer:"+primary+" Error!!!");
            return null;
        }
        
        [EntitySystem]
        private static void Destroy(this Avatar2DComponent self)
        {
            self.AvatarObjs?.Clear();
            self.AvatarParts?.Clear();
        }
    }
}

