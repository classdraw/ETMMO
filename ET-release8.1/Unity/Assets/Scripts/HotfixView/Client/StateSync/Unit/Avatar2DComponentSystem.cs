using System;
using System.Collections.Generic;
using ET;
using UnityEngine;

namespace ET.Client
{
    [FriendOf(typeof(Avatar2DComponent))]
    [FriendOf(typeof(RoleAvatarParts))]
    [EntitySystemOf(typeof(Avatar2DComponent))]
    public static partial class Avatar2DComponentSystem
    {
        private const string AvatarBundleLocationPrefix = "Assets/Bundles/Avatar/";

        [EntitySystem]
        private static void Awake(this Avatar2DComponent self, GameObject go)
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

                    partDict[partType] = partObj;
                }

                self.AvatarParts[avatarType] = partDict;
            }

            self.AvatarObjs[AvatarType.Horse].SetActive(false);
            self.AvatarObjs[AvatarType.Unit].SetActive(true);

            //self.ChangeAvatar(2010).Coroutine();
        }

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
    }
}
