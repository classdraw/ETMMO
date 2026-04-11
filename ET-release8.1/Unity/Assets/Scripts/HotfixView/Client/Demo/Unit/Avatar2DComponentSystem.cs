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
                ReferenceSpriteCollector partsCollector = avatarObj.GetComponent<ReferenceSpriteCollector>();
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
        private static SpriteRenderer GetPartSpriteRenderer(ReferenceSpriteCollector partsCollector, AvatarPartType partType)
        {
            string primary = partType.ToString();
            SpriteRenderer ro = partsCollector.Get<SpriteRenderer>(primary);
            if (ro != null)
            {
                return ro;
            }
            
            return null;
        }
        
        [EntitySystem]
        private static void Destroy(this Avatar2DComponent self)
        {
            self.AvatarObjs?.Clear();
            self.AvatarParts?.Clear();
        }
        
        
        /**
         *int avatarConfigId = 2001;
if (AvatarConfigCategory.Instance.Contain(avatarConfigId))
{
    AvatarConfig cfg = AvatarConfigCategory.Instance.Get(avatarConfigId);
    int type = cfg.AvatarType;
    string name = cfg.Name;
    string model = cfg.Model;   // 表里「模型」字段，一般是资源/预制体相对路径
}
else
{
    // 没有该 Id 时不要直接 Get，否则会抛异常
}
         *
         * 
         */
    }
}

