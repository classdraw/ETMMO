using System;
using System.Collections.Generic;
using ET;
using UnityEngine;

namespace ET.Client
{
    [FriendOf(typeof(Avatar2DComponent))]
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

        /// <summary>
        /// 按 AvatarConfig 配表换装：加载 Model 预制体实例，将其中各部位 SpriteRenderer 的贴图等同步到角色 AvatarParts 绑点。
        /// </summary>
        public static async ETTask ChangeAvatar(this Avatar2DComponent self, int avatarConfigId)
        {
            if (!AvatarConfigCategory.Instance.Contain(avatarConfigId))
            {
                Log.Warning($"ChangeAvatar: AvatarConfig 不存在 id={avatarConfigId}");
                return;
            }
            ResourcesLoaderComponent resLoader = self.Scene().GetComponent<ResourcesLoaderComponent>();
            if (resLoader == null)
            {
                Log.Warning("ChangeAvatar: CurrentScene 无 ResourcesLoaderComponent");
                return;
            }


            AvatarConfig cfg = AvatarConfigCategory.Instance.Get(avatarConfigId);
            AvatarPartType partType = (AvatarPartType)cfg.AvatarPartType;
            if (!self.AvatarParts.TryGetValue(AvatarType.Unit, out Dictionary<AvatarPartType, SpriteRenderer> unitParts) || unitParts == null)
            {
                Log.Warning("ChangeAvatar: Unit AvatarParts 未初始化");
                return;
            }

            if (!AvatarEyePairUtility.HasBindPointForAvatarPart(unitParts, partType))
            {
                return;
            }

            string location = "Assets/Bundles/Avatar/" + cfg.Model;
            GameObject prefab = await resLoader.LoadAssetAsync<GameObject>(location);
            if (prefab == null)
            {
                Log.Error($"ChangeAvatar: 加载失败 location={location}");
                return;
            }

            SpriteRenderer prefabSr = prefab.GetComponent<SpriteRenderer>();
            if (prefabSr == null)
            {
                Log.Error($"ChangeAvatar: 预制体无 SpriteRenderer location={location}");
                return;
            }

            Sprite sprite = prefabSr.sprite;
            if (AvatarEyePairUtility.IsEyePairPart(partType))
            {
                AvatarEyePairUtility.ApplySpriteToEyePair(unitParts, partType, sprite);
            }
            else if (unitParts.TryGetValue(partType, out SpriteRenderer spriteRenderer))
            {
                spriteRenderer.sprite = sprite;
            }

            await ETTask.CompletedTask;
        }
        
    }
}
